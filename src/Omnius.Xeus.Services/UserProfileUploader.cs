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
using Omnius.Core.Extensions;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;
using Omnius.Xeus.Daemon;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Services.Internal.Models;
using Omnius.Xeus.Services.Internal.Repositories;
using Omnius.Xeus.Services.Models;

namespace Omnius.Xeus.Services
{
    public sealed class UserProfileUploader : AsyncDisposableBase, IUserProfileUploader
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly UserProfileUploaderOptions _options;
        private readonly IXeusService _xeusService;
        private readonly IBytesStorageFactory _bytesStorageFactory;
        private readonly IBytesPool _bytesPool;

        private readonly UserProfileUploaderRepository _userProfileUploaderRepo;

        private Task _watchTask = null!;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly AsyncReaderWriterLock _asyncLock = new();

        private const string Registrant = "Omnius.Xeus.Services.UserProfileUploader";

        internal sealed class UserProfileUploaderFactory : IUserProfileUploaderFactory
        {
            public async ValueTask<IUserProfileUploader> CreateAsync(UserProfileUploaderOptions options, IXeusService xeusService,
                IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, CancellationToken cancellationToken = default)
            {
                var result = new UserProfileUploader(options, xeusService, bytesStorageFactory, bytesPool);
                await result.InitAsync(cancellationToken);

                return result;
            }
        }

        public static IUserProfileUploaderFactory Factory { get; } = new UserProfileUploaderFactory();

        public UserProfileUploader(UserProfileUploaderOptions options, IXeusService xeusService,
            IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool)
        {
            _options = options;
            _xeusService = xeusService;
            _bytesStorageFactory = bytesStorageFactory;
            _bytesPool = bytesPool;

            _userProfileUploaderRepo = new UserProfileUploaderRepository(Path.Combine(options.ConfigDirectoryPath, "state"));
        }

        internal async ValueTask InitAsync(CancellationToken cancellationToken = default)
        {
            _watchTask = this.WatchAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();

            await _watchTask;

            _cancellationTokenSource.Dispose();
        }

        private async Task WatchAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Delay(1, cancellationToken).ConfigureAwait(false);

                for (; ; )
                {
                    await Task.Delay(1000 * 30, cancellationToken);

                    await this.SyncPublishedDeclaredMessage(cancellationToken);
                    await this.SyncPublishedFileStorage(cancellationToken);
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

        private async Task SyncPublishedDeclaredMessage(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var publishedItems = await this.InternalGetPublishedDeclaredMessageReportsAsync(cancellationToken);
                var set = new HashSet<OmniSignature>();
                set.UnionWith(publishedItems.Where(n => n.Registrant == Registrant).Select(n => n.Signature));

                foreach (var signature in set)
                {
                    if (_userProfileUploaderRepo.Items.Exists(signature)) continue;
                    await this.InternalUnpublishDeclaredMessageAsync(signature, cancellationToken);
                }

                foreach (var signature in _userProfileUploaderRepo.Items.FindAll().Select(n => n.Signature))
                {
                    if (set.Contains(signature)) continue;
                    _userProfileUploaderRepo.Items.Delete(signature);
                }
            }
        }

        private async Task SyncPublishedFileStorage(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var publishedItems = await this.InternalGetPublishedContentReportsAsync(cancellationToken);
                var set = new HashSet<OmniHash>();
                set.UnionWith(publishedItems.Where(n => n.Registrant == Registrant).Select(n => n.RootHash).Where(n => n.HasValue).Select(n => n!.Value));

                foreach (var rootHash in set)
                {
                    if (_userProfileUploaderRepo.Items.Exists(rootHash)) continue;
                    await this.InternalUnpublishContentAsync(rootHash, cancellationToken);
                }

                foreach (var rootHash in _userProfileUploaderRepo.Items.FindAll().Select(n => n.RootHash))
                {
                    if (set.Contains(rootHash)) continue;
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
                    reports.Add(new UploadingUserProfileReport(item.Signature, item.CreationTime));
                }

                return reports;
            }
        }

        public async ValueTask RegisterAsync(UserProfileContent content, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                var rootHash = await this.InternalPublishContentAsync(content, cancellationToken);
                await this.InternalPublishDeclaredMessageAsync(rootHash, digitalSignature, cancellationToken);

                var item = new UploadingUserProfileItem(digitalSignature.GetOmniSignature(), rootHash, Timestamp.FromDateTime(DateTime.UtcNow));
                _userProfileUploaderRepo.Items.Upsert(item);
            }
        }

        public async ValueTask UnregisterAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                var item = _userProfileUploaderRepo.Items.FindOne(signature);
                if (item is null) return;

                await this.InternalUnpublishDeclaredMessageAsync(item.Signature, cancellationToken);
                await this.InternalUnpublishContentAsync(item.RootHash, cancellationToken);

                _userProfileUploaderRepo.Items.Delete(item.Signature);
            }
        }

        private async ValueTask<IEnumerable<PublishedShoutStorageReport>> InternalGetPublishedDeclaredMessageReportsAsync(CancellationToken cancellationToken = default)
        {
            var output = await _xeusService.PublishedDeclaredMessage_GetReportAsync(cancellationToken);
            return output.Report.PublishedDeclaredMessages;
        }

        private async ValueTask<IEnumerable<PublishedContentReport>> InternalGetPublishedContentReportsAsync(CancellationToken cancellationToken = default)
        {
            var output = await _xeusService.PublishedFileStorage_GetReportAsync(cancellationToken);
            return output.Report.PublishedContents;
        }

        private async ValueTask InternalPublishDeclaredMessageAsync(OmniHash rootHash, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default)
        {
            using var hub = new BytesPipe();
            rootHash.Export(hub.Writer, _bytesPool);

            var sequence = hub.Reader.GetSequence();
            var memoryOwner = _bytesPool.Memory.Rent((int)sequence.Length).Shrink((int)sequence.Length);
            sequence.CopyTo(memoryOwner.Memory.Span);

            using var declaredMessage = Shout.Create(Timestamp.FromDateTime(DateTime.UtcNow), memoryOwner, digitalSignature);
            var input = new PublishedDeclaredMessage_PublishMessage_Input(declaredMessage, Registrant);
            await _xeusService.PublishedDeclaredMessage_PublishMessageAsync(input, cancellationToken);
        }

        private async ValueTask<OmniHash> InternalPublishContentAsync(UserProfileContent content, CancellationToken cancellationToken = default)
        {
            using var hub = new BytesPipe();
            content.Export(hub.Writer, _bytesPool);

            var sequence = hub.Reader.GetSequence();
            using var memoryOwner = _bytesPool.Memory.Rent((int)sequence.Length).Shrink((int)sequence.Length);
            sequence.CopyTo(memoryOwner.Memory.Span);

            var input = new PublishedFileStorage_PublishContent_Memory_Input(memoryOwner.Memory, Registrant);
            var output = await _xeusService.PublishedFileStorage_PublishContentAsync(input, cancellationToken);
            return output.Hash;
        }

        private async ValueTask InternalUnpublishDeclaredMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var input = new PublishedDeclaredMessage_UnpublishMessage_Input(signature, Registrant);
            await _xeusService.PublishedDeclaredMessage_UnpublishMessageAsync(input, cancellationToken);
        }

        private async ValueTask InternalUnpublishContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            var input = new PublishedFileStorage_UnpublishContent_Memory_Input(rootHash, Registrant);
            await _xeusService.PublishedFileStorage_UnpublishContentAsync(input, cancellationToken);
        }
    }
}