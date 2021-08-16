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
using Omnius.Xeus.Daemon;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Services.Internal.Models;
using Omnius.Xeus.Services.Internal.Repositories;
using Omnius.Xeus.Services.Models;

namespace Omnius.Xeus.Services
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
        private const string Registrant = "Omnius.Xeus.Services.UserProfileDownloader";

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

                    await this.SyncSubscribedDeclaredMessage(cancellationToken);
                    await this.SyncSubscribedFileStorage(cancellationToken);
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

        private async Task SyncSubscribedDeclaredMessage(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var subscribedItems = await this.InternalGetSubscribedDeclaredMessageReportsAsync(cancellationToken);
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

                    var rootHash = OmniHash.Import(new ReadOnlySequence<byte>(declaredMessage.Value), _bytesPool);
                    var newItem = new DownloadingUserProfileItem(item.Signature, rootHash, declaredMessage.CreationTime);
                    _userProfileDownloaderRepo.Items.Upsert(newItem);
                }
            }
        }

        private async Task SyncSubscribedFileStorage(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var subscribedItems = await this.InternalGetSubscribedContentReportsAsync(cancellationToken);
                var subscribedRootHashSet = new HashSet<OmniHash>();
                subscribedRootHashSet.UnionWith(subscribedItems.Where(n => n.Registrant == Registrant).Select(n => n.RootHash).Where(n => n.HasValue).Select(n => n!.Value));

                foreach (var rootHash in subscribedRootHashSet)
                {
                    if (_userProfileDownloaderRepo.Items.Exists(rootHash)) continue;
                    await this.InternalUnsubscribeContentAsync(rootHash, cancellationToken);
                }

                foreach (var rootHash in _userProfileDownloaderRepo.Items.FindAll().Select(n => n.RootHash))
                {
                    if (subscribedRootHashSet.Contains(rootHash)) continue;
                    await this.InternalSubscribeContentAsync(rootHash, cancellationToken);
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

        public async ValueTask<UserProfile?> ExportAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                await Task.Delay(0, cancellationToken).ConfigureAwait(false);

                var declaredMessage = await this.InternalExportDeclaredMessageAsync(signature, cancellationToken);
                if (declaredMessage is null) return null;

                var rootHash = OmniHash.Import(new ReadOnlySequence<byte>(declaredMessage.Value), _bytesPool);
                var content = await this.InternalExportContentAsync(rootHash, cancellationToken);
                if (content is null) return null;

                return new UserProfile(signature, declaredMessage.CreationTime, content);
            }
        }

        private async ValueTask<IEnumerable<SubscribedDeclaredMessageReport>> InternalGetSubscribedDeclaredMessageReportsAsync(CancellationToken cancellationToken = default)
        {
            var output = await _xeusService.SubscribedDeclaredMessage_GetReportAsync(cancellationToken);
            return output.Report.SubscribedDeclaredMessages;
        }

        private async ValueTask<IEnumerable<SubscribedContentReport>> InternalGetSubscribedContentReportsAsync(CancellationToken cancellationToken = default)
        {
            var output = await _xeusService.SubscribedFileStorage_GetReportAsync(cancellationToken);
            return output.Report.SubscribedContents;
        }

        private async ValueTask InternalSubscribeDeclaredMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var input = new SubscribedDeclaredMessage_SubscribeMessage_Input(signature, Registrant);
            await _xeusService.SubscribedDeclaredMessage_SubscribeMessageAsync(input, cancellationToken);
        }

        private async ValueTask InternalSubscribeContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            var input = new SubscribedFileStorage_SubscribeContent_Input(rootHash, Registrant);
            await _xeusService.SubscribedFileStorage_SubscribeContentAsync(input, cancellationToken);
        }

        private async ValueTask InternalUnsubscribeDeclaredMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var input = new SubscribedDeclaredMessage_UnsubscribeMessage_Input(signature, Registrant);
            await _xeusService.SubscribedDeclaredMessage_UnsubscribeMessageAsync(input, cancellationToken);
        }

        private async ValueTask InternalUnsubscribeContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            var input = new SubscribedFileStorage_UnsubscribeContent_Input(rootHash, Registrant);
            await _xeusService.SubscribedFileStorage_UnsubscribeContentAsync(input, cancellationToken);
        }

        private async ValueTask<Shout?> InternalExportDeclaredMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var input = new SubscribedDeclaredMessage_ExportMessage_Input(signature);
            var output = await _xeusService.SubscribedDeclaredMessage_ExportMessageAsync(input, cancellationToken);
            return output.DeclaredMessage;
        }

        private async ValueTask<UserProfileContent?> InternalExportContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            var input = new SubscribedFileStorage_ExportContent_Memory_Input(rootHash);
            var output = await _xeusService.SubscribedFileStorage_ExportContentAsync(input, cancellationToken);
            if (output.Memory is null) return null;
            return UserProfileContent.Import(new ReadOnlySequence<byte>(output.Memory.Value), _bytesPool);
        }
    }
}
