using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;
using Omnius.Xeus.Intaractors.Internal;
using Omnius.Xeus.Intaractors.Internal.Models;
using Omnius.Xeus.Intaractors.Internal.Repositories;
using Omnius.Xeus.Intaractors.Models;
using Omnius.Xeus.Service.Remoting;

namespace Omnius.Xeus.Intaractors
{
    public record DownloadingUserProfileReport
    {
        public DownloadingUserProfileReport(DateTime creationTime, OmniSignature signature)
        {
            this.CreationTime = creationTime;
            this.Signature = signature;
        }

        public DateTime CreationTime { get; }

        public OmniSignature Signature { get; }
    }

    public sealed class UserProfileDownloader : AsyncDisposableBase, IUserProfileDownloader
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly XeusServiceAdapter _service;
        private readonly IBytesStorageFactory _bytesStorageFactory;
        private readonly IBytesPool _bytesPool;
        private readonly UserProfileDownloaderOptions _options;

        private readonly UserProfileDownloaderRepository _userProfileDownloaderRepo;

        private readonly Task _watchLoopTask;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly AsyncReaderWriterLock _asyncLock = new();

        private const string Registrant = "Omnius.Xeus.Intaractors.UserProfileDownloader";

        public UserProfileDownloader(IXeusService xeusService, IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, UserProfileDownloaderOptions options)
        {
            _service = new XeusServiceAdapter(xeusService);
            _bytesStorageFactory = bytesStorageFactory;
            _bytesPool = bytesPool;
            _options = options;

            _userProfileDownloaderRepo = new UserProfileDownloaderRepository(Path.Combine(_options.ConfigDirectoryPath, "state"));
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

                    await this.SyncSubscribedShouts(cancellationToken);
                    await this.SyncSubscribedFiles(cancellationToken);
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

        private async Task SyncSubscribedShouts(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var subscribedShoutReports = await _service.GetSubscribedShoutReportsAsync(cancellationToken);
                var signatures = new HashSet<OmniSignature>();
                signatures.UnionWith(subscribedShoutReports.Where(n => n.Registrant == Registrant).Select(n => n.Signature));

                foreach (var signature in signatures)
                {
                    if (_userProfileDownloaderRepo.Items.Exists(signature))
                    {
                        continue;
                    }

                    await _service.UnsubscribeShoutAsync(signature, Registrant, cancellationToken);
                }

                foreach (var item in _userProfileDownloaderRepo.Items.FindAll())
                {
                    if (signatures.Contains(item.Signature))
                    {
                        continue;
                    }

                    await _service.SubscribeShoutAsync(item.Signature, Registrant, cancellationToken);
                }

                foreach (var item in _userProfileDownloaderRepo.Items.FindAll())
                {
                    var shout = await _service.TryExportShoutAsync(item.Signature, cancellationToken);
                    if (shout is null)
                    {
                        continue;
                    }

                    var contentRootHash = RocketMessage.FromBytes<OmniHash>(shout.Value.Memory);
                    shout.Value.Dispose();

                    var newItem = new DownloadingUserProfileItem(item.Signature, contentRootHash, shout.CreationTime);
                    _userProfileDownloaderRepo.Items.Upsert(newItem);
                }
            }
        }

        private async Task SyncSubscribedFiles(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var subscribedFileReports = await _service.GetSubscribedFileReportsAsync(cancellationToken);
                var rootHashes = new HashSet<OmniHash>();
                rootHashes.UnionWith(subscribedFileReports.Where(n => n.Registrant == Registrant).Select(n => n.RootHash).Where(n => n.HasValue).Select(n => n!.Value));

                foreach (var rootHash in rootHashes)
                {
                    if (_userProfileDownloaderRepo.Items.Exists(rootHash))
                    {
                        continue;
                    }

                    await _service.UnpublishFileFromMemoryAsync(rootHash, Registrant, cancellationToken);
                }

                foreach (var rootHash in _userProfileDownloaderRepo.Items.FindAll().Select(n => n.RootHash))
                {
                    if (rootHashes.Contains(rootHash))
                    {
                        continue;
                    }

                    await _service.SubscribeFileAsync(rootHash, Registrant, cancellationToken);
                }
            }
        }

        public async ValueTask<IEnumerable<DownloadingUserProfileReport>> GetDownloadingUserProfileReportsAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var reports = new List<DownloadingUserProfileReport>();

                foreach (var item in _userProfileDownloaderRepo.Items.FindAll())
                {
                    reports.Add(new DownloadingUserProfileReport(item.CreationTime.ToDateTime(), item.Signature));
                }

                return reports;
            }
        }

        public async ValueTask RegisterAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                await Task.Delay(0, cancellationToken).ConfigureAwait(false);

                if (_userProfileDownloaderRepo.Items.Exists(signature))
                {
                    return;
                }

                var item = new DownloadingUserProfileItem(signature, OmniHash.Empty, Timestamp.FromDateTime(DateTime.UtcNow));
                _userProfileDownloaderRepo.Items.Upsert(item);
            }
        }

        public async ValueTask UnregisterAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                await Task.Delay(0, cancellationToken).ConfigureAwait(false);

                _userProfileDownloaderRepo.Items.Delete(signature);
            }
        }

        public async ValueTask<UserProfile?> ExportAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                await Task.Delay(0, cancellationToken).ConfigureAwait(false);

                var shout = await _service.TryExportShoutAsync(signature, cancellationToken);
                if (shout is null)
                {
                    return null;
                }

                var contentRootHash = RocketMessage.FromBytes<OmniHash>(shout.Value.Memory);
                shout.Value.Dispose();

                var contentBytes = await _service.TryExportFileToMemoryAsync(contentRootHash, cancellationToken);
                if (contentBytes is null)
                {
                    return null;
                }

                var content = RocketMessage.FromBytes<UserProfileContent>(contentBytes.Value);

                return new UserProfile(signature, shout.CreationTime, content);
            }
        }
    }
}
