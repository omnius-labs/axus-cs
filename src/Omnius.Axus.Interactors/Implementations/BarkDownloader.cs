using System.Collections.Immutable;
using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Internal.Repositories;
using Omnius.Axus.Interactors.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;

namespace Omnius.Axus.Interactors;

public sealed partial class BarkDownloader : AsyncDisposableBase, IBarkDownloader
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IProfileDownloader _profileDownloader;
    private readonly IAxusServiceMediator _serviceMediator;
    private readonly IBytesPool _bytesPool;
    private readonly BarkDownloaderOptions _options;

    private readonly BarkDownloaderRepository _barkDownloaderRepo;
    private readonly CachedBarkMessageRepository _cachedBarkMessageRepo;
    private readonly ISingleValueStorage _configStorage;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string Channel = "bark/v1";
    private const string Author = "bark-downloader-v1";

    public static async ValueTask<BarkDownloader> CreateAsync(IProfileDownloader profileDownloader, IAxusServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, BarkDownloaderOptions options, CancellationToken cancellationToken = default)
    {
        var barkDownloader = new BarkDownloader(profileDownloader, serviceMediator, singleValueStorageFactory, keyValueStorageFactory, bytesPool, options);
        await barkDownloader.InitAsync(cancellationToken);
        return barkDownloader;
    }

    private BarkDownloader(IProfileDownloader profileDownloader, IAxusServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, BarkDownloaderOptions options)
    {
        _profileDownloader = profileDownloader;
        _serviceMediator = serviceMediator;
        _bytesPool = bytesPool;
        _options = options;

        _barkDownloaderRepo = new BarkDownloaderRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
        _configStorage = singleValueStorageFactory.Create(Path.Combine(_options.ConfigDirectoryPath, "config"), _bytesPool);
        _cachedBarkMessageRepo = new CachedBarkMessageRepository(Path.Combine(_options.ConfigDirectoryPath, "cached_bark_messages"), _bytesPool);
    }

    internal async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        await _barkDownloaderRepo.MigrateAsync(cancellationToken);

        _watchLoopTask = this.WatchLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await _watchLoopTask;
        _cancellationTokenSource.Dispose();

        _barkDownloaderRepo.Dispose();
        _configStorage.Dispose();
    }

    private async Task WatchLoopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);

            for (; ; )
            {
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken).ConfigureAwait(false);

                var signatures = await this.GetSignaturesAsync(cancellationToken);

                await this.SyncBarkDownloaderRepo(signatures, cancellationToken);
                await this.SyncSubscribedShouts(cancellationToken);
                await this.SyncSubscribedFiles(cancellationToken);

                await this.UpdateCachedBarkMessagesAsync(signatures, cancellationToken);
                _cachedBarkMessageRepo.Shrink(signatures);
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e, "Operation Canceled");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }
    }

    private async ValueTask<ImmutableHashSet<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var builder = ImmutableHashSet.CreateBuilder<OmniSignature>();

            foreach (var signature in await _profileDownloader.GetSignaturesAsync(cancellationToken))
            {
                builder.Add(signature);
            }

            return builder.ToImmutable();
        }
    }

    private async ValueTask SyncBarkDownloaderRepo(ImmutableHashSet<OmniSignature> targetSignatures, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            foreach (var profileItem in _barkDownloaderRepo.BarkItems.FindAll())
            {
                if (targetSignatures.Contains(profileItem.Signature)) continue;
                _barkDownloaderRepo.BarkItems.Delete(profileItem.Signature);
            }

            foreach (var signature in targetSignatures)
            {
                if (_barkDownloaderRepo.BarkItems.Exists(signature)) continue;

                var newBarkItem = new DownloadingBarkItem()
                {
                    Signature = signature,
                    ShoutUpdatedTime = DateTime.MinValue,
                };
                _barkDownloaderRepo.BarkItems.Upsert(newBarkItem);
            }
        }
    }

    private async ValueTask SyncSubscribedShouts(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetSubscribedShoutReportsAsync(Author, cancellationToken);
            var signatures = reports.Select(n => n.Signature).ToHashSet();

            foreach (var signature in signatures)
            {
                if (_barkDownloaderRepo.BarkItems.Exists(signature)) continue;
                await _serviceMediator.UnsubscribeShoutAsync(signature, Channel, Author, cancellationToken);
            }

            foreach (var barkItem in _barkDownloaderRepo.BarkItems.FindAll())
            {
                if (signatures.Contains(barkItem.Signature)) continue;
                await _serviceMediator.SubscribeShoutAsync(barkItem.Signature, Channel, Author, cancellationToken);
            }

            foreach (var barkItem in _barkDownloaderRepo.BarkItems.FindAll())
            {
                using var shout = await _serviceMediator.TryExportShoutAsync(barkItem.Signature, Channel, barkItem.ShoutUpdatedTime, cancellationToken);
                if (shout is null) continue;

                var rootHash = RocketMessage.FromBytes<OmniHash>(shout.Value.Memory);

                var newBarkItem = barkItem with
                {
                    RootHash = rootHash,
                    ShoutUpdatedTime = shout.UpdatedTime.ToDateTime(),
                };
                _barkDownloaderRepo.BarkItems.Upsert(newBarkItem);
            }
        }
    }

    private async ValueTask SyncSubscribedFiles(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetSubscribedFileReportsAsync(Author, cancellationToken);
            var rootHashes = reports.Select(n => n.RootHash).ToHashSet();

            foreach (var rootHash in rootHashes)
            {
                if (_barkDownloaderRepo.BarkItems.Exists(rootHash)) continue;
                await _serviceMediator.UnpublishFileFromMemoryAsync(rootHash, Author, cancellationToken);
            }

            foreach (var rootHash in _barkDownloaderRepo.BarkItems.FindAll().Select(n => n.RootHash))
            {
                if (rootHash == OmniHash.Empty) continue;
                if (rootHashes.Contains(rootHash)) continue;
                await _serviceMediator.SubscribeFileAsync(rootHash, Author, cancellationToken);
            }
        }
    }

    private async ValueTask UpdateCachedBarkMessagesAsync(ImmutableHashSet<OmniSignature> targetSignatures, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            foreach (var signature in targetSignatures)
            {
                var cachedContentShoutUpdatedTime = _cachedBarkMessageRepo.FetchShoutUpdatedTime(signature);

                var cachedContent = await this.TryInternalExportAsync(signature, cachedContentShoutUpdatedTime, cancellationToken);
                if (cachedContent is null) continue;

                _cachedBarkMessageRepo.UpsertBulk(cachedContent);
            }
        }
    }

    private async ValueTask<CachedBarkContent?> TryInternalExportAsync(OmniSignature signature, DateTime cachedContentShoutUpdatedTime, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await Task.Delay(0, cancellationToken).ConfigureAwait(false);

            var barkItem = _barkDownloaderRepo.BarkItems.FindOne(signature);
            if (barkItem is null || barkItem.RootHash == OmniHash.Empty || barkItem.ShoutUpdatedTime <= cachedContentShoutUpdatedTime) return null;

            using var contentBytes = await _serviceMediator.TryExportFileToMemoryAsync(barkItem.RootHash, cancellationToken);
            if (contentBytes is null) return null;

            var content = RocketMessage.FromBytes<BarkContent>(contentBytes.Memory);

            var cachedContent = new CachedBarkContent(signature, Timestamp64.FromDateTime(barkItem.ShoutUpdatedTime), content);
            return cachedContent;
        }
    }

    public async ValueTask<IEnumerable<BarkMessageReport>> FindMessagesByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var results = _cachedBarkMessageRepo.FetchMessageByTag(tag);
            return results.Select(n => n.ToReport());
        }
    }

    public async ValueTask<BarkMessageReport?> FindMessageBySelfHashAsync(OmniHash selfHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var result = _cachedBarkMessageRepo.FetchMessageBySelfHash(selfHash);
            return result?.ToReport();
        }
    }

    public async ValueTask<BarkDownloaderConfig> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var config = await _configStorage.TryGetValueAsync<BarkDownloaderConfig>(cancellationToken);

            if (config is null)
            {
                config = new BarkDownloaderConfig(
                    tags: Array.Empty<Utf8String>(),
                    maxBarkMessageCount: 10000
                );

                await _configStorage.TrySetValueAsync(config, cancellationToken);
            }

            return config;
        }
    }

    public async ValueTask SetConfigAsync(BarkDownloaderConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _configStorage.TrySetValueAsync(config, cancellationToken);
        }
    }
}
