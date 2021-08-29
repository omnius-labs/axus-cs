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
using Omnius.Xeus.Service.Remoting;
using Omnius.Xeus.Intaractors.Internal;
using Omnius.Xeus.Intaractors.Internal.Models;
using Omnius.Xeus.Intaractors.Internal.Repositories;
using Omnius.Xeus.Intaractors.Models;

namespace Omnius.Xeus.Intaractors
{
    public sealed class UserProfileDownloader : AsyncDisposableBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly XeusService _xeusServiceAdapter;
        private readonly IBytesStorageFactory _bytesStorageFactory;
        private readonly IBytesPool _bytesPool;
        private readonly UserProfileDownloaderOptions _options;

        private readonly UserProfileDownloaderRepository _repo;

        private readonly Task _watchTask;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly AsyncReaderWriterLock _asyncLock = new();
        private const string Registrant = "Omnius.Xeus.Intaractors.UserProfileDownloader";

        public UserProfileDownloader(IXeusService xeusService, IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, UserProfileDownloaderOptions options)
        {
            _options = options;
            _xeusServiceAdapter = new XeusServiceAdapter(xeusService, bytesPool);
            _bytesStorageFactory = bytesStorageFactory;
            _bytesPool = bytesPool;

            _repo = new UserProfileDownloaderRepository(Path.Combine(_options.ConfigDirectoryPath, "state"));

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
                var subscribedShoutReports = await _xeusServiceAdapter.GetSubscribedShoutsReportAsync(cancellationToken);
                var subscribedSignatureSet = new HashSet<OmniSignature>();
                subscribedSignatureSet.UnionWith(subscribedShoutReports.Where(n => n.Registrant == Registrant).Select(n => n.Signature));

                foreach (var signature in subscribedSignatureSet)
                {
                    if (_repo.Items.Exists(signature)) continue;
                    await _xeusServiceAdapter.UnsubscribeShoutAsync(signature, Registrant, cancellationToken);
                }

                foreach (var item in _repo.Items.FindAll())
                {
                    if (subscribedSignatureSet.Contains(item.Signature)) continue;
                    await this.InternalSubscribeDeclaredMessageAsync(item.Signature, cancellationToken);
                }

                foreach (var item in _repo.Items.FindAll())
                {
                    var declaredMessage = await this.InternalExportDeclaredMessageAsync(item.Signature, cancellationToken);
                    if (declaredMessage is null) continue;

                    var rootHash = OmniHash.Import(new ReadOnlySequence<byte>(declaredMessage.Value), _bytesPool);
                    var newItem = new DownloadingUserProfileItem(item.Signature, rootHash, declaredMessage.CreationTime);
                    _repo.Items.Upsert(newItem);
                }
            }
        }

        private async Task SyncSubscribedFiles(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var subscribedItems = await this.InternalGetSubscribedReportsAsync(cancellationToken);
                var subscribedRootHashSet = new HashSet<OmniHash>();
                subscribedRootHashSet.UnionWith(subscribedItems.Where(n => n.Registrant == Registrant).Select(n => n.RootHash).Where(n => n.HasValue).Select(n => n!.Value));

                foreach (var rootHash in subscribedRootHashSet)
                {
                    if (_repo.Items.Exists(rootHash)) continue;
                    await this.InternalUnsubscribeContentAsync(rootHash, cancellationToken);
                }

                foreach (var rootHash in _repo.Items.FindAll().Select(n => n.RootHash))
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

                foreach (var item in _repo.Items.FindAll())
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

                if (_repo.Items.Exists(signature)) return;

                var item = new DownloadingUserProfileItem(signature, OmniHash.Empty, Timestamp.FromDateTime(DateTime.UtcNow));
                _repo.Items.Upsert(item);
            }
        }

        public async ValueTask UnregisterAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                await Task.Delay(0, cancellationToken).ConfigureAwait(false);

                _repo.Items.Delete(signature);
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

    }
}
