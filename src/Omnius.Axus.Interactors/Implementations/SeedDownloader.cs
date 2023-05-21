using System.Collections.Immutable;
using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Internal.Repositories;
using Omnius.Axus.Interactors.Models;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;

namespace Omnius.Axus.Interactors;

public sealed partial class SeedDownloader : AsyncDisposableBase, ISeedDownloader
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IProfileDownloader _profileDownloader;
    private readonly IAxusServiceMediator _serviceMediator;
    private readonly IBytesPool _bytesPool;
    private readonly SeedDownloaderOptions _options;

    private readonly CachedSeedBoxRepository _cachedSeedBoxRepo;
    private readonly ISingleValueStorage _configStorage;
    private SeedDownloaderConfig? _config;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string PROPERTIES_SHOUT = "Shout";

    private const string Channel = "seed-v1";
    private const string Zone = "seed-downloader-v1";

    public static async ValueTask<SeedDownloader> CreateAsync(IProfileDownloader profileDownloader, IAxusServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, SeedDownloaderOptions options, CancellationToken cancellationToken = default)
    {
        var seedDownloader = new SeedDownloader(profileDownloader, serviceMediator, singleValueStorageFactory, keyValueStorageFactory, bytesPool, options);
        await seedDownloader.InitAsync(cancellationToken);
        return seedDownloader;
    }

    private SeedDownloader(IProfileDownloader profileDownloader, IAxusServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, SeedDownloaderOptions options)
    {
        _profileDownloader = profileDownloader;
        _serviceMediator = serviceMediator;
        _bytesPool = bytesPool;
        _options = options;

        _cachedSeedBoxRepo = new CachedSeedBoxRepository(Path.Combine(_options.ConfigDirectoryPath, "cached_seed_boxes"), _bytesPool);
        _configStorage = singleValueStorageFactory.Create(Path.Combine(_options.ConfigDirectoryPath, "config"), _bytesPool);
    }

    internal async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        await _cachedSeedBoxRepo.MigrateAsync(cancellationToken);

        _watchLoopTask = this.WatchLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await _watchLoopTask;
        _cancellationTokenSource.Dispose();

        _configStorage.Dispose();
    }

    private async Task WatchLoopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);

            for (; ; )
            {
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken).ConfigureAwait(false);

                await this.SyncAsync(cancellationToken);
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

    private async ValueTask SyncAsync(CancellationToken cancellationToken = default)
    {
        // 1. 不要なSubscribedShoutを削除
        // 2. 不要なSubscribedFileを削除
        // 3. 不要なCachedSeedBoxを削除
        // 4. 新しいSubscribedShoutを追加
        // 5. 新しいSubscribedFileを追加
        // 6. 新しいCachedSeedBoxを追加

        var targetSignatures = await this.GetSignaturesAsync(cancellationToken);

        var subscribedShoutKeys = await this.TryRemoveUnusedSubscribedShoutsAsync(targetSignatures, cancellationToken);
        var subscribedFileKeys = await this.TryRemoveUnusedSubscribedFilesAsync(subscribedShoutKeys, cancellationToken);
        var cachedSeedBoxKeys = await this.TryRemoveUnusedCachedSeedBoxesAsync(subscribedFileKeys, cancellationToken);

        await this.TryAddSubscribedShoutsAsync(targetSignatures, subscribedShoutKeys, cancellationToken);
        await this.TryAddSubscribedFilesAsync(subscribedShoutKeys, subscribedFileKeys, cancellationToken);
        await this.TryAddCachedSeedBoxesAsync(subscribedFileKeys, cachedSeedBoxKeys, cancellationToken);
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

    private async ValueTask<ImmutableDictionary<OmniSignature, Timestamp64>> TryRemoveUnusedSubscribedShoutsAsync(ImmutableHashSet<OmniSignature> targetSignatures, CancellationToken cancellationToken = default)
    {
        var builder = ImmutableDictionary.CreateBuilder<OmniSignature, Timestamp64>();

        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetSubscribedShoutReportsAsync(Zone, cancellationToken);

            foreach (var report in reports)
            {
                if (targetSignatures.Contains(report.Signature))
                {
                    builder.Add(report.Signature, report.CreatedTime);
                    continue;
                }

                await _serviceMediator.UnsubscribeShoutAsync(report.Signature, Channel, Zone, cancellationToken);
            }
        }

        return builder.ToImmutable();
    }

    private async ValueTask<ImmutableDictionary<OmniSignature, (Timestamp64, OmniHash)>> TryRemoveUnusedSubscribedFilesAsync(ImmutableDictionary<OmniSignature, Timestamp64> subscribedShoutKeys, CancellationToken cancellationToken = default)
    {
        var builder = ImmutableDictionary.CreateBuilder<OmniSignature, (Timestamp64, OmniHash)>();

        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetSubscribedFileReportsAsync(Zone, cancellationToken);

            foreach (var report in reports)
            {
                if (report.Properties.TryGetValue<Shout>(PROPERTIES_SHOUT, out var shout)
                    && shout.Certificate is not null)
                {
                    if (subscribedShoutKeys.TryGetValue(shout.Certificate.GetOmniSignature(), out var targetCreatedTime)
                        && targetCreatedTime == shout.CreatedTime)
                    {
                        builder.Add(shout.Certificate.GetOmniSignature(), (shout.CreatedTime, report.RootHash));
                        continue;
                    }
                }

                await _serviceMediator.UnsubscribeFileAsync(report.RootHash, Zone, cancellationToken);
            }
        }

        return builder.ToImmutable();
    }

    private async ValueTask<ImmutableDictionary<OmniSignature, Timestamp64>> TryRemoveUnusedCachedSeedBoxesAsync(ImmutableDictionary<OmniSignature, (Timestamp64, OmniHash)> subscribedFileKeys, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var signatures = subscribedFileKeys.Select(n => n.Key).ToList();
            await _cachedSeedBoxRepo.ShrinkAsync(signatures, cancellationToken);

            var cachedBoxSeedKeys = await _cachedSeedBoxRepo.GetKeysAsync(cancellationToken);

            var builder = ImmutableDictionary.CreateBuilder<OmniSignature, Timestamp64>();
            foreach (var (signature, createdTime) in cachedBoxSeedKeys)
            {
                builder.Add(signature, createdTime);
            }

            return builder.ToImmutable();
        }
    }

    private async ValueTask TryAddSubscribedShoutsAsync(ImmutableHashSet<OmniSignature> targetSignatures, ImmutableDictionary<OmniSignature, Timestamp64> subscribedShoutKeys, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            foreach (var signature in targetSignatures)
            {
                if (subscribedShoutKeys.Contains(signature)) continue;

                await _serviceMediator.SubscribeShoutAsync(signature, Channel, Enumerable.Empty<AttachedProperty>(), Zone, cancellationToken);
            }
        }
    }

    private async ValueTask TryAddSubscribedFilesAsync(ImmutableDictionary<OmniSignature, Timestamp64> subscribedShoutKeys, ImmutableDictionary<OmniSignature, (Timestamp64, OmniHash)> subscribedFileKeys, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            foreach (var (signature, createdTime) in subscribedShoutKeys)
            {
                if (subscribedFileKeys.ContainsKey(signature)) continue;

                using var shout = await _serviceMediator.TryExportShoutAsync(signature, Channel, createdTime, cancellationToken);
                if (shout is null) continue;

                var rootHash = RocketMessage.FromBytes<OmniHash>(shout.Value.Memory);

                using var shoutBytes = RocketMessage.ToBytes(shout);
                var property = new AttachedProperty(PROPERTIES_SHOUT, shoutBytes.Memory);

                await _serviceMediator.SubscribeFileAsync(rootHash, new[] { property }, Zone, cancellationToken);
            }
        }
    }

    private async ValueTask TryAddCachedSeedBoxesAsync(ImmutableDictionary<OmniSignature, (Timestamp64, OmniHash)> subscribedFileKeys, ImmutableDictionary<OmniSignature, Timestamp64> cachedSeedBoxKeys, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            foreach (var (signature, (createdTime, rootHash)) in subscribedFileKeys)
            {
                if (cachedSeedBoxKeys.TryGetValue(signature, out var cachedCreatedTime)
                    && createdTime <= cachedCreatedTime) continue;

                using var seedBoxBytes = await _serviceMediator.TryExportFileToMemoryAsync(rootHash, cancellationToken);
                if (seedBoxBytes is null) continue;

                var seedBox = RocketMessage.FromBytes<SeedBox>(seedBoxBytes.Memory);
                var cachedSeedBox = new CachedSeedBox(signature, createdTime, seedBox);
                await _cachedSeedBoxRepo.UpsertAsync(cachedSeedBox, cancellationToken);
            }
        }
    }


    public async ValueTask<SeedDownloaderConfig> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            if (_config is not null) return _config;

            _config = await _configStorage.TryGetValueAsync<SeedDownloaderConfig>(cancellationToken);

            if (_config is null)
            {
                _config = new SeedDownloaderConfig(
                    maxSeedBoxCount: 10000
                );

                await _configStorage.TrySetValueAsync(_config, cancellationToken);
            }

            return _config;
        }
    }

    public async ValueTask SetConfigAsync(SeedDownloaderConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _configStorage.TrySetValueAsync(config, cancellationToken);
            _config = config;
        }
    }

    public ValueTask<FindSeedsResult> FindSeedsAsync(FindSeedsCondition condition, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
