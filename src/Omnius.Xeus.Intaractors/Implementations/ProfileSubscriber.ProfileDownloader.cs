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

namespace Omnius.Xeus.Intaractors
{
    public sealed partial class ProfileSubscriber
    {
        internal sealed class ProfileDownloader : AsyncDisposableBase
        {
            private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

            private readonly XeusServiceAdapter _service;
            private readonly IKeyValueStorageFactory _keyValueStorageFactory;
            private readonly IBytesPool _bytesPool;
            private readonly string _configDirectoryPath;

            private readonly ProfileDownloaderRepository _profileDownloaderRepo;

            private Task _watchLoopTask = null!;

            private readonly CancellationTokenSource _cancellationTokenSource = new();

            private readonly AsyncReaderWriterLock _asyncLock = new();

            public static async ValueTask<ProfileDownloader> CreateAsync(XeusServiceAdapter service, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, string configDirectoryPath, CancellationToken cancellationToken = default)
            {
                var profileDownloader = new ProfileDownloader(service, keyValueStorageFactory, bytesPool, configDirectoryPath);
                await profileDownloader.InitAsync(cancellationToken);
                return profileDownloader;
            }

            private ProfileDownloader(XeusServiceAdapter service, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, string configDirectoryPath)
            {
                _service = service;
                _keyValueStorageFactory = keyValueStorageFactory;
                _bytesPool = bytesPool;
                _configDirectoryPath = configDirectoryPath;

                _profileDownloaderRepo = new ProfileDownloaderRepository(Path.Combine(_configDirectoryPath, "state"));
            }

            internal async ValueTask InitAsync(CancellationToken cancellationToken = default)
            {
                await _profileDownloaderRepo.MigrateAsync(cancellationToken);

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
                        if (_profileDownloaderRepo.Items.Exists(signature)) continue;

                        await _service.UnsubscribeShoutAsync(signature, Registrant, cancellationToken);
                    }

                    foreach (var item in _profileDownloaderRepo.Items.FindAll())
                    {
                        if (signatures.Contains(item.Signature)) continue;

                        await _service.SubscribeShoutAsync(item.Signature, Registrant, cancellationToken);
                    }

                    foreach (var item in _profileDownloaderRepo.Items.FindAll())
                    {
                        var shout = await _service.TryExportShoutAsync(item.Signature, cancellationToken);
                        if (shout is null) continue;

                        var contentRootHash = RocketMessage.FromBytes<OmniHash>(shout.Value.Memory);
                        shout.Value.Dispose();

                        var newItem = new DownloadingProfileItem(item.Signature, contentRootHash, shout.CreationTime.ToDateTime());
                        _profileDownloaderRepo.Items.Upsert(newItem);
                    }
                }
            }

            private async Task SyncSubscribedFiles(CancellationToken cancellationToken = default)
            {
                using (await _asyncLock.ReaderLockAsync(cancellationToken))
                {
                    var subscribedFileReports = await _service.GetSubscribedFileReportsAsync(cancellationToken);
                    var rootHashes = new HashSet<OmniHash>();
                    rootHashes.UnionWith(subscribedFileReports.Where(n => n.Registrant == Registrant).Select(n => n.RootHash));

                    foreach (var rootHash in rootHashes)
                    {
                        if (_profileDownloaderRepo.Items.Exists(rootHash)) continue;

                        await _service.UnpublishFileFromMemoryAsync(rootHash, Registrant, cancellationToken);
                    }

                    foreach (var rootHash in _profileDownloaderRepo.Items.FindAll().Select(n => n.RootHash))
                    {
                        if (rootHashes.Contains(rootHash)) continue;

                        await _service.SubscribeFileAsync(rootHash, Registrant, cancellationToken);
                    }
                }
            }

            public async ValueTask<IEnumerable<OmniSignature>> GetDownloadingProfileSignaturesAsync(CancellationToken cancellationToken = default)
            {
                using (await _asyncLock.ReaderLockAsync(cancellationToken))
                {
                    var results = new List<OmniSignature>();

                    foreach (var item in _profileDownloaderRepo.Items.FindAll())
                    {
                        results.Add(item.Signature);
                    }

                    return results;
                }
            }

            public async ValueTask RegisterAsync(OmniSignature signature, CancellationToken cancellationToken = default)
            {
                using (await _asyncLock.WriterLockAsync(cancellationToken))
                {
                    await Task.Delay(0, cancellationToken).ConfigureAwait(false);

                    if (_profileDownloaderRepo.Items.Exists(signature)) return;

                    var item = new DownloadingProfileItem(signature, OmniHash.Empty, DateTime.UtcNow);
                    _profileDownloaderRepo.Items.Upsert(item);
                }
            }

            public async ValueTask UnregisterAsync(OmniSignature signature, CancellationToken cancellationToken = default)
            {
                using (await _asyncLock.WriterLockAsync(cancellationToken))
                {
                    await Task.Delay(0, cancellationToken).ConfigureAwait(false);

                    _profileDownloaderRepo.Items.Delete(signature);
                }
            }

            public async ValueTask<Profile?> ExportAsync(OmniSignature signature, CancellationToken cancellationToken = default)
            {
                using (await _asyncLock.WriterLockAsync(cancellationToken))
                {
                    await Task.Delay(0, cancellationToken).ConfigureAwait(false);

                    var shout = await _service.TryExportShoutAsync(signature, cancellationToken);
                    if (shout is null) return null;

                    var contentRootHash = RocketMessage.FromBytes<OmniHash>(shout.Value.Memory);
                    shout.Value.Dispose();

                    var contentBytes = await _service.TryExportFileToMemoryAsync(contentRootHash, cancellationToken);
                    if (contentBytes is null) return null;

                    var content = RocketMessage.FromBytes<ProfileContent>(contentBytes.Value);

                    return new Profile(signature, shout.CreationTime, content);
                }
            }
        }
    }
}
