using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Pipelines;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;
using Omnius.Xeus.Intaractors.Internal;
using Omnius.Xeus.Intaractors.Internal.Models;
using Omnius.Xeus.Intaractors.Internal.Repositories;
using Omnius.Xeus.Intaractors.Models;
using Omnius.Xeus.Service.Models;
using Omnius.Xeus.Service.Remoting;

namespace Omnius.Xeus.Intaractors
{
    public record UploadingUserProfileReport
    {
        public UploadingUserProfileReport(DateTime creationTime, OmniSignature signature)
        {
            this.CreationTime = creationTime;
            this.Signature = signature;
        }

        public DateTime CreationTime { get; }

        public OmniSignature Signature { get; }
    }

    public sealed class UserProfileUploader : AsyncDisposableBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly XeusServiceAdapter _service;
        private readonly IBytesStorageFactory _bytesStorageFactory;
        private readonly IBytesPool _bytesPool;
        private readonly UserProfileUploaderOptions _options;

        private readonly UserProfileUploaderRepository _userProfileUploaderRepo;

        private readonly Task _watchLoopTask;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly AsyncReaderWriterLock _asyncLock = new();

        private const string Registrant = "Omnius.Xeus.Intaractors.UserProfileUploader";

        public UserProfileUploader(IXeusService xeusService, IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, UserProfileUploaderOptions options)
        {
            _service = new XeusServiceAdapter(xeusService);
            _bytesStorageFactory = bytesStorageFactory;
            _bytesPool = bytesPool;
            _options = options;

            _userProfileUploaderRepo = new UserProfileUploaderRepository(Path.Combine(_options.ConfigDirectoryPath, "state"));
            _watchLoopTask = this.WatchLoopAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();

            await _watchLoopTask;

            _cancellationTokenSource.Dispose();
        }

        private async Task WatchLoopAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Delay(1, cancellationToken).ConfigureAwait(false);

                for (; ; )
                {
                    await Task.Delay(1000 * 30, cancellationToken);

                    await this.SyncPublishedShouts(cancellationToken);
                    await this.SyncPublishedFiles(cancellationToken);
                }
            }
            catch (OperationCanceledException e)
            {
                _logger.Debug(e);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private async Task SyncPublishedShouts(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var publishedShoutReports = await _service.GetPublishedShoutReportsAsync(cancellationToken);
                var signatures = new HashSet<OmniSignature>();
                signatures.UnionWith(publishedShoutReports.Where(n => n.Registrant == Registrant).Select(n => n.Signature));

                foreach (var signature in signatures)
                {
                    if (_userProfileUploaderRepo.Items.Exists(signature)) continue;
                    await _service.UnpublishShoutAsync(signature, Registrant, cancellationToken);
                }

                foreach (var item in _userProfileUploaderRepo.Items.FindAll())
                {
                    if (signatures.Contains(item.Signature)) continue;
                    _userProfileUploaderRepo.Items.Delete(item.Signature);
                }
            }
        }

        private async Task SyncPublishedFiles(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var publishedFileReports = await _service.GetPublishedFileReportsAsync(cancellationToken);
                var rootHashes = new HashSet<OmniHash>();
                rootHashes.UnionWith(publishedFileReports.Where(n => n.Registrant == Registrant).Select(n => n.RootHash).Where(n => n.HasValue).Select(n => n!.Value));

                foreach (var rootHash in rootHashes)
                {
                    if (_userProfileUploaderRepo.Items.Exists(rootHash)) continue;
                    await _service.UnpublishFileFromMemoryAsync(rootHash, Registrant, cancellationToken);
                }

                foreach (var rootHash in _userProfileUploaderRepo.Items.FindAll().Select(n => n.RootHash))
                {
                    if (rootHashes.Contains(rootHash)) continue;
                    _userProfileUploaderRepo.Items.Delete(rootHash);
                }
            }
        }

        public async ValueTask<IEnumerable<UploadingUserProfileReport>> GetUploadingUserProfileReportsAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var reports = new List<UploadingUserProfileReport>();

                foreach (var item in _userProfileUploaderRepo.Items.FindAll())
                {
                    reports.Add(new UploadingUserProfileReport(item.CreationTime.ToDateTime(), item.Signature));
                }

                return reports;
            }
        }

        public async ValueTask RegisterAsync(UserProfileContent content, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                using var contentBytes = RocketMessage.ToBytes(content);

                var contentRootHash = await _service.PublishFileFromMemoryAsync(contentBytes.Memory, Registrant, cancellationToken);

                var shout = Shout.Create(Timestamp.FromDateTime(DateTime.UtcNow), RocketMessage.ToBytes(contentRootHash), digitalSignature);
                await _service.PublishShoutAsync(shout, Registrant, cancellationToken);
                shout.Value.Dispose();

                var item = new UploadingUserProfileItem(digitalSignature.GetOmniSignature(), contentRootHash, Timestamp.FromDateTime(DateTime.UtcNow));
                _userProfileUploaderRepo.Items.Upsert(item);
            }
        }

        public async ValueTask UnregisterAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                var item = _userProfileUploaderRepo.Items.FindOne(signature);
                if (item is null) return;

                await _service.UnpublishShoutAsync(item.Signature, Registrant, cancellationToken);
                await _service.UnpublishFileFromMemoryAsync(item.RootHash, Registrant, cancellationToken);

                _userProfileUploaderRepo.Items.Delete(item.Signature);
            }
        }
    }
}
