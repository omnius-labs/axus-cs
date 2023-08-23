using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Internal.Repositories;
using Omnius.Axus.Interactors.Models;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;

namespace Omnius.Axus.Interactors;

public sealed partial class ProfileDownloader : AsyncDisposableBase, IProfileDownloader
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IAxusServiceMediator _serviceMediator;
    private readonly IBytesPool _bytesPool;
    private readonly ProfileDownloaderOptions _options;

    private readonly CachedProfileRepository _cachedProfileRepo;
    private readonly ISingleValueStorage _configStorage;
    private ProfileDownloaderConfig? _config;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string PROPERTIES_SHOUT = "Shout";

    private const string Channel = "profile-v1";
    private const string Zone = "profile-downloader-v1";

    public static async ValueTask<ProfileDownloader> CreateAsync(IAxusServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, ProfileDownloaderOptions options, CancellationToken cancellationToken = default)
    {
        var profileDownloader = new ProfileDownloader(serviceMediator, singleValueStorageFactory, keyValueStorageFactory, bytesPool, options);
        await profileDownloader.InitAsync(cancellationToken);
        return profileDownloader;
    }

    private ProfileDownloader(IAxusServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, ProfileDownloaderOptions options)
    {
        _serviceMediator = serviceMediator;
        _bytesPool = bytesPool;
        _options = options;

        _cachedProfileRepo = new CachedProfileRepository(Path.Combine(_options.ConfigDirectoryPath, "cached_profile_contents"), bytesPool);
        _configStorage = singleValueStorageFactory.Create(Path.Combine(_options.ConfigDirectoryPath, "config"), _bytesPool);
    }

    internal async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        await _cachedProfileRepo.MigrateAsync(cancellationToken);

        _watchLoopTask = this.WatchLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await _watchLoopTask;
        _cancellationTokenSource.Dispose();

        await _configStorage.DisposeAsync();
    }

    private async Task WatchLoopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);

            for (; ; )
            {
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken).ConfigureAwait(false);

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

    private async ValueTask SyncAsync(CancellationToken cancellationToken)
    {
        // 1. 不要なSubscribedShoutを削除
        // 2. 不要なSubscribedFileを削除
        // 3. 不要なCachedProfileを削除
        // 4. 新しいSubscribedShoutを追加
        // 5. 新しいSubscribedFileを追加
        // 6. 新しいCachedProfileを追加

        var targetSignatures = await this.InternalGetSignaturesAsync(cancellationToken);

        var subscribedShoutKeys = await this.TryRemoveUnusedSubscribedShoutsAsync(targetSignatures, cancellationToken);
        var subscribedFileKeys = await this.TryRemoveUnusedSubscribedFilesAsync(subscribedShoutKeys, cancellationToken);
        var cachedProfileKeys = await this.TryRemoveUnusedCachedProfilesAsync(subscribedFileKeys, cancellationToken);

        await this.TryAddSubscribedShoutsAsync(targetSignatures, subscribedShoutKeys, cancellationToken);
        await this.TryAddSubscribedFilesAsync(subscribedShoutKeys, subscribedFileKeys, cancellationToken);
        await this.TryAddCachedProfilesAsync(subscribedFileKeys, cachedProfileKeys, cancellationToken);
    }

    private async ValueTask<ImmutableHashSet<OmniSignature>> InternalGetSignaturesAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var config = await this.GetConfigAsync(cancellationToken);
            return await this.InternalFindProfilesAsync(config.TrustedSignatures, config.BlockedSignatures, 3, 30000, cancellationToken);
        }
    }

    private async ValueTask<ImmutableHashSet<OmniSignature>> InternalFindProfilesAsync(
        IEnumerable<OmniSignature> rootSignatures, IEnumerable<OmniSignature> ignoreSignatures, int depth, int maxCount, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var builder = ImmutableHashSet.CreateBuilder<OmniSignature>();

            var targetSignatures = new HashSet<OmniSignature>();
            var checkedSignatures = new HashSet<OmniSignature>();

            targetSignatures.UnionWith(rootSignatures);
            checkedSignatures.UnionWith(ignoreSignatures);

            int count = 0;

            foreach (int rank in Enumerable.Range(0, depth))
            {
                if (targetSignatures.Count == 0) break;

                var nextTargetSignatures = new HashSet<OmniSignature>();

                await foreach (var profile in this.InternalFindProfilesAsync(targetSignatures, cancellationToken))
                {
                    checkedSignatures.Add(profile.Signature);
                    checkedSignatures.UnionWith(profile.Value.BlockedSignatures);
                    nextTargetSignatures.UnionWith(profile.Value.TrustedSignatures);

                    builder.Add(profile.Signature);

                    if (++count >= maxCount) break;
                }

                nextTargetSignatures.ExceptWith(checkedSignatures);
                targetSignatures = nextTargetSignatures;
            }

            return builder.ToImmutable();
        }
    }

    private async IAsyncEnumerable<CachedProfile> InternalFindProfilesAsync(IEnumerable<OmniSignature> signatures, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            foreach (var signature in signatures)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var cachedProfile = await _cachedProfileRepo.TryGetCachedProfileAsync(signature, cancellationToken);
                if (cachedProfile is null) continue;

                yield return cachedProfile;
            }
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

    private async ValueTask<ImmutableDictionary<OmniSignature, Timestamp64>> TryRemoveUnusedCachedProfilesAsync(ImmutableDictionary<OmniSignature, (Timestamp64, OmniHash)> subscribedFileKeys, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var signatures = subscribedFileKeys.Select(n => n.Key).ToList();
            await _cachedProfileRepo.ShrinkAsync(signatures, cancellationToken);

            var cachedProfileKeys = await _cachedProfileRepo.GetKeysAsync(cancellationToken);

            var builder = ImmutableDictionary.CreateBuilder<OmniSignature, Timestamp64>();
            foreach (var (signature, createdTime) in cachedProfileKeys)
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

    private async ValueTask TryAddCachedProfilesAsync(ImmutableDictionary<OmniSignature, (Timestamp64, OmniHash)> subscribedFileKeys, ImmutableDictionary<OmniSignature, Timestamp64> cachedProfileKeys, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            foreach (var (signature, (createdTime, rootHash)) in subscribedFileKeys)
            {
                if (cachedProfileKeys.TryGetValue(signature, out var cachedCreatedTime)
                    && createdTime <= cachedCreatedTime) continue;

                using var profileBytes = await _serviceMediator.TryExportFileToMemoryAsync(rootHash, cancellationToken);
                if (profileBytes is null) continue;

                var profile = RocketMessage.FromBytes<Profile>(profileBytes.Memory);
                var cachedProfile = new CachedProfile(signature, createdTime, profile);
                await _cachedProfileRepo.UpsertAsync(cachedProfile, cancellationToken);
            }
        }
    }

    public async ValueTask<ProfileDownloaderConfig> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            if (_config is not null) return _config;

            _config = await _configStorage.TryGetValueAsync<ProfileDownloaderConfig>(cancellationToken);

            if (_config is null)
            {
                _config = new ProfileDownloaderConfig(
                    trustedSignatures: Array.Empty<OmniSignature>(),
                    blockedSignatures: Array.Empty<OmniSignature>()
                );

                await _configStorage.SetValueAsync(_config, cancellationToken);
            }

            return _config;
        }
    }

    public async ValueTask SetConfigAsync(ProfileDownloaderConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _configStorage.SetValueAsync(config, cancellationToken);
            _config = config;
        }
    }

    public ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public ValueTask<ProfileReport?> FindProfileBySignatureAsync(OmniSignature signature, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
