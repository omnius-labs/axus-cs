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
using Omnius.Xeus.Api;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Interactors.Internal.Models;
using Omnius.Xeus.Interactors.Internal.Repositories;
using Omnius.Xeus.Interactors.Models;

namespace Omnius.Xeus.Interactors
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

        private const string Registrant = "Omnius.Xeus.Interactors.UserProfileUploader";

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

                    await this.SyncDeclaredMessagePublisher(cancellationToken);
                    await this.SyncContentPublisher(cancellationToken);
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

        private async Task SyncDeclaredMessagePublisher(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var publishedItems = await this.InternalGetDeclaredMessagePublishedItemReportsAsync(cancellationToken);
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

        private async Task SyncContentPublisher(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var publishedItems = await this.InternalGetContentPublishedItemReportsAsync(cancellationToken);
                var set = new HashSet<OmniHash>();
                set.UnionWith(publishedItems.Where(n => n.Registrant == Registrant).Select(n => n.ContentHash).Where(n => n.HasValue).Select(n => n!.Value));

                foreach (var contentHash in set)
                {
                    if (_userProfileUploaderRepo.Items.Exists(contentHash)) continue;
                    await this.InternalUnpublishContentAsync(contentHash, cancellationToken);
                }

                foreach (var contentHash in _userProfileUploaderRepo.Items.FindAll().Select(n => n.ContentHash))
                {
                    if (set.Contains(contentHash)) continue;
                    _userProfileUploaderRepo.Items.Delete(contentHash);
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

        public async ValueTask RegisterAsync(XeusUserProfileContent content, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                var contentHash = await this.InternalPublishContentAsync(content, cancellationToken);
                await this.InternalPublishDeclaredMessageAsync(contentHash, digitalSignature, cancellationToken);

                var item = new UploadingUserProfileItem(digitalSignature.GetOmniSignature(), contentHash, Timestamp.FromDateTime(DateTime.UtcNow));
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
                await this.InternalUnpublishContentAsync(item.ContentHash, cancellationToken);

                _userProfileUploaderRepo.Items.Delete(item.Signature);
            }
        }

        private async ValueTask<IEnumerable<DeclaredMessagePublishedItemReport>> InternalGetDeclaredMessagePublishedItemReportsAsync(CancellationToken cancellationToken = default)
        {
            var output = await _xeusService.DeclaredMessagePublisher_GetReportAsync(cancellationToken);
            return output.Report.DeclaredMessagePublishedItems;
        }

        private async ValueTask<IEnumerable<ContentPublishedItemReport>> InternalGetContentPublishedItemReportsAsync(CancellationToken cancellationToken = default)
        {
            var output = await _xeusService.ContentPublisher_GetReportAsync(cancellationToken);
            return output.Report.ContentPublishedItems;
        }

        private async ValueTask InternalPublishDeclaredMessageAsync(OmniHash contentHash, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default)
        {
            using var hub = new BytesHub();
            contentHash.Export(hub.Writer, _bytesPool);

            var sequence = hub.Reader.GetSequence();
            var memoryOwner = _bytesPool.Memory.Rent((int)sequence.Length).Shrink((int)sequence.Length);
            sequence.CopyTo(memoryOwner.Memory.Span);

            using var declaredMessage = DeclaredMessage.Create(Timestamp.FromDateTime(DateTime.UtcNow), memoryOwner, digitalSignature);
            var input = new DeclaredMessagePublisher_PublishMessage_Input(declaredMessage, Registrant);
            await _xeusService.DeclaredMessagePublisher_PublishMessageAsync(input, cancellationToken);
        }

        private async ValueTask<OmniHash> InternalPublishContentAsync(XeusUserProfileContent content, CancellationToken cancellationToken = default)
        {
            using var hub = new BytesHub();
            content.Export(hub.Writer, _bytesPool);

            var sequence = hub.Reader.GetSequence();
            using var memoryOwner = _bytesPool.Memory.Rent((int)sequence.Length).Shrink((int)sequence.Length);
            sequence.CopyTo(memoryOwner.Memory.Span);

            var input = new ContentPublisher_PublishContent_Memory_Input(memoryOwner.Memory, Registrant);
            var output = await _xeusService.ContentPublisher_PublishContentAsync(input, cancellationToken);
            return output.Hash;
        }

        private async ValueTask InternalUnpublishDeclaredMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var input = new DeclaredMessagePublisher_UnpublishMessage_Input(signature, Registrant);
            await _xeusService.DeclaredMessagePublisher_UnpublishMessageAsync(input, cancellationToken);
        }

        private async ValueTask InternalUnpublishContentAsync(OmniHash contentHash, CancellationToken cancellationToken = default)
        {
            var input = new ContentPublisher_UnpublishContent_Memory_Input(contentHash, Registrant);
            await _xeusService.ContentPublisher_UnpublishContentAsync(input, cancellationToken);
        }
    }
}
