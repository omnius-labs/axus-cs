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
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;
using Omnius.Xeus.Api;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Interactors.Internal.Models;
using Omnius.Xeus.Interactors.Internal.Repositories;
using Omnius.Xeus.Interactors.Models;

namespace Omnius.Xeus.Interactors
{
    public sealed class UserProfileDownloader : AsyncDisposableBase, IUserProfileDownloader
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly UserProfileDownloaderOptions _options;
        private readonly IXeusService _xeusService;
        private readonly IBytesStorageFactory _bytesStorageFactory;
        private readonly IBytesPool _bytesPool;

        private readonly UserProfileDownloaderRepository _userProfileDownloaderRepo;

        private Task _watchTask = null!;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly AsyncReaderWriterLock _asyncLock = new();
        private const string Registrant = "Omnius.Xeus.Interactors.UserProfileDownloader";

        internal sealed class UserProfileDownloaderFactory : IUserProfileDownloaderFactory
        {
            public async ValueTask<IUserProfileDownloader> CreateAsync(UserProfileDownloaderOptions options, IXeusService xeusService,
                IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, CancellationToken cancellationToken = default)
            {
                var result = new UserProfileDownloader(options, xeusService, bytesStorageFactory, bytesPool);
                await result.InitAsync(cancellationToken);

                return result;
            }
        }

        public static IUserProfileDownloaderFactory Factory { get; } = new UserProfileDownloaderFactory();

        public UserProfileDownloader(UserProfileDownloaderOptions options, IXeusService xeusService,
            IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool)
        {
            _options = options;
            _xeusService = xeusService;
            _bytesStorageFactory = bytesStorageFactory;
            _bytesPool = bytesPool;

            _userProfileDownloaderRepo = new UserProfileDownloaderRepository(Path.Combine(_options.ConfigDirectoryPath, "state"));
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

                    await this.SyncDeclaredMessageSubscriber(cancellationToken);
                    await this.SyncContentSubscriber(cancellationToken);
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

        private async Task SyncDeclaredMessageSubscriber(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var subscribedItems = await this.InternalGetDeclaredMessageSubscribedItemReportsAsync(cancellationToken);
                var subscribedSignatureSet = new HashSet<OmniSignature>();
                subscribedSignatureSet.UnionWith(subscribedItems.Where(n => n.Registrant == Registrant).Select(n => n.Signature));

                foreach (var signature in subscribedSignatureSet)
                {
                    if (_userProfileDownloaderRepo.Items.Exists(signature)) continue;
                    await this.InternalUnsubscribeDeclaredMessageAsync(signature, cancellationToken);
                }

                foreach (var item in _userProfileDownloaderRepo.Items.FindAll())
                {
                    if (subscribedSignatureSet.Contains(item.Signature)) continue;
                    await this.InternalSubscribeDeclaredMessageAsync(item.Signature, cancellationToken);
                }

                foreach (var item in _userProfileDownloaderRepo.Items.FindAll())
                {
                    var declaredMessage = await this.InternalExportDeclaredMessageAsync(item.Signature, cancellationToken);
                    if (declaredMessage is null) continue;

                    var contentHash = OmniHash.Import(new ReadOnlySequence<byte>(declaredMessage.Value), _bytesPool);
                    var newItem = new DownloadingUserProfileItem(item.Signature, contentHash, declaredMessage.CreationTime);
                    _userProfileDownloaderRepo.Items.Upsert(newItem);
                }
            }
        }

        private async Task SyncContentSubscriber(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var subscribedItems = await this.InternalGetContentSubscribedItemReportsAsync(cancellationToken);
                var subscribedContentHashSet = new HashSet<OmniHash>();
                subscribedContentHashSet.UnionWith(subscribedItems.Where(n => n.Registrant == Registrant).Select(n => n.ContentHash).Where(n => n.HasValue).Select(n => n!.Value));

                foreach (var contentHash in subscribedContentHashSet)
                {
                    if (_userProfileDownloaderRepo.Items.Exists(contentHash)) continue;
                    await this.InternalUnsubscribeContentAsync(contentHash, cancellationToken);
                }

                foreach (var contentHash in _userProfileDownloaderRepo.Items.FindAll().Select(n => n.ContentHash))
                {
                    if (subscribedContentHashSet.Contains(contentHash)) continue;
                    await this.InternalSubscribeContentAsync(contentHash, cancellationToken);
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
                    reports.Add(new DownloadingUserProfileReport(item.Signature, item.CreationTime));
                }

                return reports;
            }
        }

        public async ValueTask RegisterAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                await Task.Delay(0, cancellationToken).ConfigureAwait(false);

                if (_userProfileDownloaderRepo.Items.Exists(signature)) return;

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

        public async ValueTask<XeusUserProfile?> ExportAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                await Task.Delay(0, cancellationToken).ConfigureAwait(false);

                var declaredMessage = await this.InternalExportDeclaredMessageAsync(signature, cancellationToken);
                if (declaredMessage is null) return null;

                var contentHash = OmniHash.Import(new ReadOnlySequence<byte>(declaredMessage.Value), _bytesPool);
                var content = await this.InternalExportContentAsync(contentHash, cancellationToken);
                if (content is null) return null;

                return new XeusUserProfile(signature, declaredMessage.CreationTime, content);
            }
        }

        private async ValueTask<IEnumerable<DeclaredMessageSubscribedItemReport>> InternalGetDeclaredMessageSubscribedItemReportsAsync(CancellationToken cancellationToken = default)
        {
            var output = await _xeusService.DeclaredMessageSubscriber_GetReportAsync(cancellationToken);
            return output.Report.DeclaredMessageSubscribedItems;
        }

        private async ValueTask<IEnumerable<ContentSubscribedItemReport>> InternalGetContentSubscribedItemReportsAsync(CancellationToken cancellationToken = default)
        {
            var output = await _xeusService.ContentSubscriber_GetReportAsync(cancellationToken);
            return output.Report.ContentSubscribedItems;
        }

        private async ValueTask InternalSubscribeDeclaredMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var input = new DeclaredMessageSubscriber_SubscribeMessage_Input(signature, Registrant);
            await _xeusService.DeclaredMessageSubscriber_SubscribeMessageAsync(input, cancellationToken);
        }

        private async ValueTask InternalSubscribeContentAsync(OmniHash contentHash, CancellationToken cancellationToken = default)
        {
            var input = new ContentSubscriber_SubscribeContent_Input(contentHash, Registrant);
            await _xeusService.ContentSubscriber_SubscribeContentAsync(input, cancellationToken);
        }

        private async ValueTask InternalUnsubscribeDeclaredMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var input = new DeclaredMessageSubscriber_UnsubscribeMessage_Input(signature, Registrant);
            await _xeusService.DeclaredMessageSubscriber_UnsubscribeMessageAsync(input, cancellationToken);
        }

        private async ValueTask InternalUnsubscribeContentAsync(OmniHash contentHash, CancellationToken cancellationToken = default)
        {
            var input = new ContentSubscriber_UnsubscribeContent_Input(contentHash, Registrant);
            await _xeusService.ContentSubscriber_UnsubscribeContentAsync(input, cancellationToken);
        }

        private async ValueTask<DeclaredMessage?> InternalExportDeclaredMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var input = new DeclaredMessageSubscriber_ExportMessage_Input(signature);
            var output = await _xeusService.DeclaredMessageSubscriber_ExportMessageAsync(input, cancellationToken);
            return output.DeclaredMessage;
        }

        private async ValueTask<XeusUserProfileContent?> InternalExportContentAsync(OmniHash contentHash, CancellationToken cancellationToken = default)
        {
            var input = new ContentSubscriber_ExportContent_Memory_Input(contentHash);
            var output = await _xeusService.ContentSubscriber_ExportContentAsync(input, cancellationToken);
            if (output.Memory is null) return null;
            return XeusUserProfileContent.Import(new ReadOnlySequence<byte>(output.Memory.Value), _bytesPool);
        }
    }
}
