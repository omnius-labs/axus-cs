using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Omnius.Axis.Intaractors.Internal;
using Omnius.Axis.Intaractors.Internal.Models;
using Omnius.Axis.Intaractors.Internal.Repositories;
using Omnius.Axis.Intaractors.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;

namespace Omnius.Axis.Intaractors;

public sealed partial class ProfileSubscriber : AsyncDisposableBase, IProfileSubscriber
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IServiceController _serviceController;
    private readonly IBytesPool _bytesPool;
    private readonly ProfileSubscriberOptions _options;

    private readonly ProfileSubscriberRepository _profileSubscriberRepo;
    private readonly ISingleValueStorage _configStorage;
    private readonly IKeyValueStorage<string> _cachedProfileStorage;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string Registrant = "Omnius.Axis.Intaractors.ProfileSubscriber";

    public static async ValueTask<ProfileSubscriber> CreateAsync(IServiceController service, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, ProfileSubscriberOptions options, CancellationToken cancellationToken = default)
    {
        var profileSubscriber = new ProfileSubscriber(service, singleValueStorageFactory, keyValueStorageFactory, bytesPool, options);
        await profileSubscriber.InitAsync(cancellationToken);
        return profileSubscriber;
    }

    private ProfileSubscriber(IServiceController service, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, ProfileSubscriberOptions options)
    {
        _serviceController = service;
        _bytesPool = bytesPool;
        _options = options;

        _profileSubscriberRepo = new ProfileSubscriberRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
        _configStorage = singleValueStorageFactory.Create(Path.Combine(_options.ConfigDirectoryPath, "config"), _bytesPool);
        _cachedProfileStorage = keyValueStorageFactory.Create<string>(Path.Combine(_options.ConfigDirectoryPath, "profiles"), _bytesPool);
    }

    internal async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        await _profileSubscriberRepo.MigrateAsync(cancellationToken);
        await _cachedProfileStorage.MigrateAsync(cancellationToken);

        _watchLoopTask = this.WatchLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await _watchLoopTask;
        _cancellationTokenSource.Dispose();

        _profileSubscriberRepo.Dispose();
        _configStorage.Dispose();
        _cachedProfileStorage.Dispose();
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
                await this.UpdateProfilesAsync(cancellationToken);
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

    private async Task SyncSubscribedShouts(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var subscribedShoutReports = await _serviceController.GetSubscribedShoutReportsAsync(cancellationToken);
            var signatures = new HashSet<OmniSignature>();
            signatures.UnionWith(subscribedShoutReports.Where(n => n.Registrant == Registrant).Select(n => n.Signature));

            foreach (var signature in signatures)
            {
                if (_profileSubscriberRepo.Items.Exists(signature)) continue;

                await _serviceController.UnsubscribeShoutAsync(signature, Registrant, cancellationToken);
            }

            foreach (var item in _profileSubscriberRepo.Items.FindAll())
            {
                if (signatures.Contains(item.Signature)) continue;

                await _serviceController.SubscribeShoutAsync(item.Signature, Registrant, cancellationToken);
            }

            foreach (var item in _profileSubscriberRepo.Items.FindAll())
            {
                var shout = await _serviceController.TryExportShoutAsync(item.Signature, cancellationToken);
                if (shout is null) continue;

                var contentRootHash = RocketMessage.FromBytes<OmniHash>(shout.Value.Memory);
                shout.Value.Dispose();

                var newItem = new SubscribedProfileItem(item.Signature, contentRootHash, shout.CreatedTime.ToDateTime());
                _profileSubscriberRepo.Items.Upsert(newItem);
            }
        }
    }

    private async Task SyncSubscribedFiles(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var subscribedFileReports = await _serviceController.GetSubscribedFileReportsAsync(cancellationToken);
            var rootHashes = new HashSet<OmniHash>();
            rootHashes.UnionWith(subscribedFileReports.Where(n => n.Registrant == Registrant).Select(n => n.RootHash));

            foreach (var rootHash in rootHashes)
            {
                if (_profileSubscriberRepo.Items.Exists(rootHash)) continue;

                await _serviceController.UnpublishFileFromMemoryAsync(rootHash, Registrant, cancellationToken);
            }

            foreach (var rootHash in _profileSubscriberRepo.Items.FindAll().Select(n => n.RootHash))
            {
                if (rootHashes.Contains(rootHash)) continue;

                await _serviceController.SubscribeFileAsync(rootHash, Registrant, cancellationToken);
            }
        }
    }

    private async ValueTask UpdateProfilesAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var config = await this.GetConfigAsync(cancellationToken);

            var allTargetSignaturesBuilder = ImmutableHashSet.CreateBuilder<OmniSignature>();

            await foreach (var profile in this.InternalRecursiveFindProfilesAsync(config.TrustedSignatures, config.BlockedSignatures, (int)config.SearchDepth, (int)config.MaxProfileCount, cancellationToken))
            {
                allTargetSignaturesBuilder.Add(profile.Signature);
            }

            var allTargetSignatures = allTargetSignaturesBuilder.ToImmutableHashSet();
            await this.ShrinkCachedProfileStorage(allTargetSignatures, cancellationToken);
            await this.ShrinkProfileSubscriberRepo(allTargetSignatures, cancellationToken);
        }
    }

    private async IAsyncEnumerable<Profile> InternalRecursiveFindProfilesAsync(IEnumerable<OmniSignature> rootSignatures, IEnumerable<OmniSignature> ignoreSignatures, int depth, int maxCount, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (maxCount == 0) yield break;

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
                checkedSignatures.UnionWith(profile.Content.BlockedSignatures);
                nextTargetSignatures.UnionWith(profile.Content.TrustedSignatures);

                yield return profile;

                if (++count >= maxCount) yield break;
            }

            nextTargetSignatures.ExceptWith(checkedSignatures);
            targetSignatures = nextTargetSignatures;
        }
    }

    private async IAsyncEnumerable<Profile> InternalFindProfilesAsync(IEnumerable<OmniSignature> signatures, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var signature in signatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var cachedProfile = await _cachedProfileStorage.TryGetValueAsync<Profile>(StringConverter.SignatureToString(signature), cancellationToken);
            var downloadedProfile = await this.InternalExportAsync(signature, cancellationToken);

            if (cachedProfile is not null)
            {
                if (downloadedProfile is null)
                {
                    yield return cachedProfile;
                }
                else
                {
                    yield return cachedProfile.CreatedTime <= downloadedProfile.CreatedTime ? downloadedProfile : cachedProfile;
                }
            }
            else if (downloadedProfile is not null)
            {
                yield return downloadedProfile;
            }
        }
    }

    private async ValueTask<Profile?> InternalExportAsync(OmniSignature signature, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);

        var shout = await _serviceController.TryExportShoutAsync(signature, cancellationToken);
        if (shout is null) return null;

        var contentRootHash = RocketMessage.FromBytes<OmniHash>(shout.Value.Memory);
        shout.Value.Dispose();

        var contentBytes = await _serviceController.TryExportFileToMemoryAsync(contentRootHash, cancellationToken);
        if (contentBytes is null) return null;

        var content = RocketMessage.FromBytes<ProfileContent>(contentBytes.Value);

        return new Profile(signature, shout.CreatedTime, content);
    }

    private async ValueTask ShrinkCachedProfileStorage(IEnumerable<OmniSignature> allTargetSignatures, CancellationToken cancellationToken = default)
    {
        var hashSet = allTargetSignatures.Select(n => StringConverter.SignatureToString(n)).ToImmutableHashSet();

        var removedKeys = new HashSet<string>();
        await foreach (var key in _cachedProfileStorage.GetKeysAsync(cancellationToken))
        {
            if (hashSet.Contains(key)) continue;

            removedKeys.Add(key);
        }

        foreach (var key in removedKeys)
        {
            await _cachedProfileStorage.TryDeleteAsync(key, cancellationToken);
        }
    }

    private async ValueTask ShrinkProfileSubscriberRepo(ImmutableHashSet<OmniSignature> allTargetSignatures, CancellationToken cancellationToken = default)
    {
        var removedSignatures = new HashSet<OmniSignature>();
        foreach (var item in _profileSubscriberRepo.Items.FindAll())
        {
            if (allTargetSignatures.Contains(item.Signature)) continue;

            removedSignatures.Add(item.Signature);
        }

        foreach (var key in removedSignatures)
        {
            _profileSubscriberRepo.Items.Delete(key);
        }
    }

    public async ValueTask<IEnumerable<SubscribedProfileReport>> GetSubscribedProfileReportsAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = new List<SubscribedProfileReport>();

            foreach (var item in _profileSubscriberRepo.Items.FindAll())
            {
                reports.Add(new SubscribedProfileReport(item.CreatedTime, item.Signature));
            }

            return reports;
        }
    }

    public async ValueTask<Profile?> FindOneAsync(OmniSignature signature, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            return await _cachedProfileStorage.TryGetValueAsync<Profile>(StringConverter.SignatureToString(signature), cancellationToken);
        }
    }

    public async IAsyncEnumerable<Profile> FindAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await foreach (var value in _cachedProfileStorage.GetValuesAsync<Profile>(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return value;
            }
        }
    }

    public async ValueTask<ProfileSubscriberConfig> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var config = await _configStorage.TryGetValueAsync<ProfileSubscriberConfig>(cancellationToken);

            if (config is null)
            {
                config = new ProfileSubscriberConfig(
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

    public async ValueTask SetConfigAsync(ProfileSubscriberConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _configStorage.TrySetValueAsync(config, cancellationToken);
        }
    }
}
