using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Internal.Repositories;
using Omnius.Axus.Interactors.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;

namespace Omnius.Axus.Interactors;

public sealed partial class BarkSubscriber : AsyncDisposableBase, IBarkSubscriber
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IProfileSubscriber _profileSubscriber;
    private readonly IServiceMediator _serviceMediator;
    private readonly IBytesPool _bytesPool;
    private readonly BarkSubscriberOptions _options;

    private readonly BarkSubscriberRepository _barkSubscriberRepo;
    private readonly CachedBarkMessageRepository _cachedBarkMessageRepo;
    private readonly ISingleValueStorage _configStorage;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string Channel = "bark/v1";
    private const string Registrant = "bark_subscriber/v1";

    public static async ValueTask<BarkSubscriber> CreateAsync(IProfileSubscriber profileSubscriber, IServiceMediator axusServiceMediator, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, BarkSubscriberOptions options, CancellationToken cancellationToken = default)
    {
        var barkSubscriber = new BarkSubscriber(profileSubscriber, axusServiceMediator, singleValueStorageFactory, keyValueStorageFactory, bytesPool, options);
        await barkSubscriber.InitAsync(cancellationToken);
        return barkSubscriber;
    }

    private BarkSubscriber(IProfileSubscriber profileSubscriber, IServiceMediator axusServiceMediator, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, BarkSubscriberOptions options)
    {
        _profileSubscriber = profileSubscriber;
        _serviceMediator = axusServiceMediator;
        _bytesPool = bytesPool;
        _options = options;

        _barkSubscriberRepo = new BarkSubscriberRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
        _configStorage = singleValueStorageFactory.Create(Path.Combine(_options.ConfigDirectoryPath, "config"), _bytesPool);
        _cachedBarkMessageRepo = new CachedBarkMessageRepository(Path.Combine(_options.ConfigDirectoryPath, "cached_bark_messages"), _bytesPool);
    }

    internal async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        await _barkSubscriberRepo.MigrateAsync(cancellationToken);

        _watchLoopTask = this.WatchLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await _watchLoopTask;
        _cancellationTokenSource.Dispose();

        _barkSubscriberRepo.Dispose();
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

                await this.SyncSubscribedShouts(cancellationToken);
                await this.SyncSubscribedFiles(cancellationToken);
                var excludedSignatures = await this.InternalFetchTargetSignatures(cancellationToken);
                await this.InternalShrinkAsync(excludedSignatures, cancellationToken);
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
            var reports = await _serviceMediator.GetSubscribedShoutReportsAsync(cancellationToken);
            var signatures = reports
                .Where(n => n.Registrant == Registrant)
                .Select(n => n.Signature)
                .ToHashSet();

            foreach (var signature in signatures)
            {
                if (_barkSubscriberRepo.Items.Exists(signature)) continue;
                await _serviceMediator.UnsubscribeShoutAsync(signature, Channel, Registrant, cancellationToken);
            }

            foreach (var item in _barkSubscriberRepo.Items.FindAll())
            {
                if (signatures.Contains(item.Signature)) continue;
                await _serviceMediator.SubscribeShoutAsync(item.Signature, Channel, Registrant, cancellationToken);
            }

            foreach (var item in _barkSubscriberRepo.Items.FindAll())
            {
                using var shout = await _serviceMediator.TryExportShoutAsync(item.Signature, Channel, cancellationToken);
                if (shout is null) continue;

                var rootHash = RocketMessage.FromBytes<OmniHash>(shout.Value.Memory);

                var newItem = new SubscribedBarkItem(item.Signature, rootHash, shout.CreatedTime.ToDateTime());
                _barkSubscriberRepo.Items.Upsert(newItem);
            }
        }
    }

    private async Task SyncSubscribedFiles(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetSubscribedFileReportsAsync(cancellationToken);
            var rootHashes = reports
                .Where(n => n.Authors.Contains(Registrant))
                .Select(n => n.RootHash)
                .ToHashSet();

            foreach (var rootHash in rootHashes)
            {
                if (_barkSubscriberRepo.Items.Exists(rootHash)) continue;
                await _serviceMediator.UnpublishFileFromMemoryAsync(rootHash, Registrant, cancellationToken);
            }

            foreach (var rootHash in _barkSubscriberRepo.Items.FindAll().Select(n => n.RootHash))
            {
                if (rootHashes.Contains(rootHash)) continue;
                await _serviceMediator.SubscribeFileAsync(rootHash, Registrant, cancellationToken);
            }
        }
    }

    private async ValueTask<IEnumerable<OmniSignature>> InternalFetchTargetSignatures(CancellationToken cancellationToken = default)
    {
        var signatures = new List<OmniSignature>();

        foreach (var profile in await _profileSubscriber.FindAllAsync(cancellationToken))
        {
            signatures.Add(profile.Signature);
        }

        return signatures;
    }

    private async ValueTask InternalShrinkAsync(IEnumerable<OmniSignature> excludedSignatures, CancellationToken cancellationToken = default)
    {
        _cachedBarkMessageRepo.Shrink(excludedSignatures);
        _barkSubscriberRepo.Items.Shrink(excludedSignatures);
    }

    private async ValueTask InternalFetchBarkMessagesAsync(IEnumerable<OmniSignature> targetSignatures, CancellationToken cancellationToken = default)
    {
        var config = await this.GetConfigAsync(cancellationToken);
        var tagSet = config.Tags.ToHashSet();

        foreach (var signature in targetSignatures)
        {
            foreach (var message in await this.InternalExportAsync(signature, cancellationToken))
            {
                if (!tagSet.Contains(message.Value.Tag)) continue;
            }
        }
    }

    private async ValueTask<IEnumerable<CachedBarkMessage>> InternalExportAsync(OmniSignature signature, CancellationToken cancellationToken = default)
    {
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);

        using var shout = await _serviceMediator.TryExportShoutAsync(signature, Channel, cancellationToken);
        if (shout is null) return Enumerable.Empty<CachedBarkMessage>();

        var contentRootHash = RocketMessage.FromBytes<OmniHash>(shout.Value.Memory);

        using var contentBytes = await _serviceMediator.TryExportFileToMemoryAsync(contentRootHash, cancellationToken);
        if (contentBytes is null) return Enumerable.Empty<CachedBarkMessage>();

        var content = RocketMessage.FromBytes<BarkContent>(contentBytes.Memory);

        var results = new List<CachedBarkMessage>();

        foreach (var message in content.Messages)
        {
            results.Add(new CachedBarkMessage(signature, message));
        }

        return results;
    }

    public async ValueTask<IEnumerable<BarkMessageReport>> FindByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        var results = _cachedBarkMessageRepo.FetchByTag(tag);
        return results.Select(n => n.ToReport());
    }

    public async ValueTask<BarkMessageReport?> FindBySelfHashAsync(OmniHash selfHash, CancellationToken cancellationToken = default)
    {
        var result = _cachedBarkMessageRepo.FetchBySelfHash(selfHash);
        return result?.ToReport();
    }

    public async ValueTask<BarkSubscriberConfig> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var config = await _configStorage.TryGetValueAsync<BarkSubscriberConfig>(cancellationToken);

            if (config is null)
            {
                config = new BarkSubscriberConfig(
                    tags: Array.Empty<Utf8String>(),
                    maxBarkCount: 10000
                );

                await _configStorage.TrySetValueAsync(config, cancellationToken);
            }

            return config;
        }
    }

    public async ValueTask SetConfigAsync(BarkSubscriberConfig config, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _configStorage.TrySetValueAsync(config, cancellationToken);
        }
    }
}
