using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Internal.Repositories;
using Omnius.Axus.Interactors.Models;
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

    private readonly ProfileDownloaderRepository _profileDownloaderRepo;
    private readonly ISingleValueStorage _configStorage;
    private readonly CachedProfileContentRepository _cachedProfileContentRepo;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string Channel = "profile/v1";
    private const string Author = "profile_downloader/v1";

    public static async ValueTask<ProfileDownloader> CreateAsync(IAxusServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, ProfileDownloaderOptions options, CancellationToken cancellationToken = default)
    {
        var ProfileDownloader = new ProfileDownloader(serviceMediator, singleValueStorageFactory, keyValueStorageFactory, bytesPool, options);
        await ProfileDownloader.InitAsync(cancellationToken);
        return ProfileDownloader;
    }

    private ProfileDownloader(IAxusServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, ProfileDownloaderOptions options)
    {
        _serviceMediator = serviceMediator;
        _bytesPool = bytesPool;
        _options = options;

        _profileDownloaderRepo = new ProfileDownloaderRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
        _configStorage = singleValueStorageFactory.Create(Path.Combine(_options.ConfigDirectoryPath, "config"), _bytesPool);
        _cachedProfileContentRepo = new CachedProfileContentRepository(Path.Combine(_options.ConfigDirectoryPath, "cached_profile_contents"), keyValueStorageFactory, bytesPool);
    }

    internal async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        await _profileDownloaderRepo.MigrateAsync(cancellationToken);
        await _cachedProfileContentRepo.MigrateAsync(cancellationToken);

        _watchLoopTask = this.WatchLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await _watchLoopTask;
        _cancellationTokenSource.Dispose();

        _profileDownloaderRepo.Dispose();
        _configStorage.Dispose();
        _cachedProfileContentRepo.Dispose();
    }

    private async Task WatchLoopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);

            for (; ; )
            {
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken).ConfigureAwait(false);

                var config = await this.GetConfigAsync(cancellationToken);

                var signatures = await this.UpdateCachedProfileContentsAsync(config.TrustedSignatures, config.BlockedSignatures, (int)config.SearchDepth, (int)config.MaxProfileCount, cancellationToken);
                await _cachedProfileContentRepo.ShrinkAsync(signatures);

                await this.SyncProfileDownloaderRepo(signatures, cancellationToken);
                await this.SyncSubscribedShouts(cancellationToken);
                await this.SyncSubscribedFiles(cancellationToken);
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

    private async ValueTask<ImmutableHashSet<OmniSignature>> UpdateCachedProfileContentsAsync(IEnumerable<OmniSignature> rootSignatures, IEnumerable<OmniSignature> ignoreSignatures, int depth, int maxCount, CancellationToken cancellationToken = default)
    {
        if (maxCount == 0) return ImmutableHashSet<OmniSignature>.Empty;

        var results = new List<OmniSignature>();

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

                results.Add(profile.Signature);

                if (++count >= maxCount) break;
            }

            nextTargetSignatures.ExceptWith(checkedSignatures);
            targetSignatures = nextTargetSignatures;
        }

        return results.ToImmutableHashSet();
    }

    private async IAsyncEnumerable<CachedProfileContent> InternalFindProfilesAsync(IEnumerable<OmniSignature> signatures, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var signature in signatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var cachedContent = await _cachedProfileContentRepo.TryReadAsync(signature, cancellationToken);
            var cachedContentUpdatedTime = cachedContent?.ShoutUpdatedTime.ToDateTime() ?? DateTime.MinValue;

            var downloadedContent = await this.TryInternalExportAsync(signature, cachedContentUpdatedTime, cancellationToken);

            if (downloadedContent is not null)
            {
                await _cachedProfileContentRepo.UpsertAsync(downloadedContent, cancellationToken);
                yield return downloadedContent;
            }
            else if (cachedContent is not null)
            {
                yield return cachedContent;
            }
        }
    }

    private async ValueTask<CachedProfileContent?> TryInternalExportAsync(OmniSignature signature, DateTime cachedContentUpdatedTime, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await Task.Delay(0, cancellationToken).ConfigureAwait(false);

            var shoutItem = _profileDownloaderRepo.ProfileItems.FindOne(signature);
            if (shoutItem is null || shoutItem.RootHash == OmniHash.Empty || shoutItem.ShoutUpdatedTime <= cachedContentUpdatedTime) return null;

            using var contentBytes = await _serviceMediator.TryExportFileToMemoryAsync(shoutItem.RootHash, cancellationToken);
            if (contentBytes is null) return null;

            var content = RocketMessage.FromBytes<ProfileContent>(contentBytes.Memory);

            return new CachedProfileContent(signature, Timestamp64.FromDateTime(shoutItem.ShoutUpdatedTime), content);
        }
    }

    private async ValueTask SyncProfileDownloaderRepo(ImmutableHashSet<OmniSignature> targetSignatures, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            foreach (var profileItem in _profileDownloaderRepo.ProfileItems.FindAll())
            {
                if (targetSignatures.Contains(profileItem.Signature)) continue;
                _profileDownloaderRepo.ProfileItems.Delete(profileItem.Signature);
            }

            foreach (var signature in targetSignatures)
            {
                if (_profileDownloaderRepo.ProfileItems.Exists(signature)) continue;

                var newProfileItem = new DownloadingProfileItem()
                {
                    Signature = signature,
                    ShoutUpdatedTime = DateTime.MinValue,
                };
                _profileDownloaderRepo.ProfileItems.Upsert(newProfileItem);
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
                if (_profileDownloaderRepo.ProfileItems.Exists(signature)) continue;
                await _serviceMediator.UnsubscribeShoutAsync(signature, Channel, Author, cancellationToken);
            }

            foreach (var profileItem in _profileDownloaderRepo.ProfileItems.FindAll())
            {
                if (signatures.Contains(profileItem.Signature)) continue;
                await _serviceMediator.SubscribeShoutAsync(profileItem.Signature, Channel, Author, cancellationToken);
            }

            foreach (var profileItem in _profileDownloaderRepo.ProfileItems.FindAll())
            {
                using var shout = await _serviceMediator.TryExportShoutAsync(profileItem.Signature, Channel, profileItem.ShoutUpdatedTime, cancellationToken);
                if (shout is null) continue;

                var rootHash = RocketMessage.FromBytes<OmniHash>(shout.Value.Memory);

                var newProfileItem = profileItem with
                {
                    RootHash = rootHash,
                    ShoutUpdatedTime = shout.UpdatedTime.ToDateTime()
                };
                _profileDownloaderRepo.ProfileItems.Upsert(newProfileItem);
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
                if (_profileDownloaderRepo.ProfileItems.Exists(rootHash)) continue;
                await _serviceMediator.UnpublishFileFromMemoryAsync(rootHash, Author, cancellationToken);
            }

            foreach (var rootHash in _profileDownloaderRepo.ProfileItems.FindAll().Select(n => n.RootHash))
            {
                if (rootHash == OmniHash.Empty) continue;
                if (rootHashes.Contains(rootHash)) continue;
                await _serviceMediator.SubscribeFileAsync(rootHash, Author, cancellationToken);
            }
        }
    }

    public async ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var results = new List<OmniSignature>();

            await foreach (var signature in _cachedProfileContentRepo.GetSignaturesAsync(cancellationToken))
            {
                results.Add(signature);
            }

            return results;
        }
    }

    public async ValueTask<ProfileReport?> FindProfileBySignatureAsync(OmniSignature signature, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var cachedProfile = await _cachedProfileContentRepo.TryReadAsync(signature, cancellationToken);
            if (cachedProfile is null) return null;

            var message = new ProfileReport(cachedProfile.Signature, cachedProfile.ShoutUpdatedTime.ToDateTime(), cachedProfile.Value.TrustedSignatures, cachedProfile.Value.BlockedSignatures);
            return message;
        }
    }

    public async ValueTask<ProfileDownloaderConfig> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var config = await _configStorage.TryGetValueAsync<ProfileDownloaderConfig>(cancellationToken);

            if (config is null)
            {
                config = new ProfileDownloaderConfig(
                    trustedSignatures: Array.Empty<OmniSignature>(),
                    blockedSignatures: Array.Empty<OmniSignature>(),
                    searchDepth: 128,
                    maxProfileCount: 1024
                );

                await _configStorage.TrySetValueAsync(config, cancellationToken);
            }

            return config;
        }
    }

    public async ValueTask SetConfigAsync(ProfileDownloaderConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _configStorage.TrySetValueAsync(config, cancellationToken);
        }
    }
}
