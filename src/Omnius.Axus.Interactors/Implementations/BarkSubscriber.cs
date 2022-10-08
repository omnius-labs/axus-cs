using System.Collections.Immutable;
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
    private const string Author = "bark_subscriber/v1";

    public static async ValueTask<BarkSubscriber> CreateAsync(IProfileSubscriber profileSubscriber, IServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, BarkSubscriberOptions options, CancellationToken cancellationToken = default)
    {
        var barkSubscriber = new BarkSubscriber(profileSubscriber, serviceMediator, singleValueStorageFactory, keyValueStorageFactory, bytesPool, options);
        await barkSubscriber.InitAsync(cancellationToken);
        return barkSubscriber;
    }

    private BarkSubscriber(IProfileSubscriber profileSubscriber, IServiceMediator serviceMediator, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, BarkSubscriberOptions options)
    {
        _profileSubscriber = profileSubscriber;
        _serviceMediator = serviceMediator;
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

                var signatures = await this.GetSignaturesAsync(cancellationToken);

                await this.SyncBarkSubscriberRepo(signatures, cancellationToken);
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
            var signatures = await _profileSubscriber.GetSignaturesAsync(cancellationToken);
            return signatures.ToImmutableHashSet();
        }
    }

    private async ValueTask SyncBarkSubscriberRepo(ImmutableHashSet<OmniSignature> targetSignatures, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            foreach (var profileItem in _barkSubscriberRepo.BarkItems.FindAll())
            {
                if (targetSignatures.Contains(profileItem.Signature)) continue;
                _barkSubscriberRepo.BarkItems.Delete(profileItem.Signature);
            }

            foreach (var signature in targetSignatures)
            {
                if (_barkSubscriberRepo.BarkItems.Exists(signature)) continue;

                var newBarkItem = new SubscribedBarkItem()
                {
                    Signature = signature,
                    ShoutUpdatedTime = DateTime.MinValue,
                };
                _barkSubscriberRepo.BarkItems.Upsert(newBarkItem);
            }
        }
    }

    private async ValueTask SyncSubscribedShouts(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetSubscribedShoutReportsAsync(cancellationToken);
            var signatures = reports
                .Where(n => n.Authors.Contains(Author))
                .Select(n => n.Signature)
                .ToHashSet();

            foreach (var signature in signatures)
            {
                if (_barkSubscriberRepo.BarkItems.Exists(signature)) continue;
                await _serviceMediator.UnsubscribeShoutAsync(signature, Channel, Author, cancellationToken);
            }

            foreach (var barkItem in _barkSubscriberRepo.BarkItems.FindAll())
            {
                if (signatures.Contains(barkItem.Signature)) continue;
                await _serviceMediator.SubscribeShoutAsync(barkItem.Signature, Channel, Author, cancellationToken);
            }

            foreach (var barkItem in _barkSubscriberRepo.BarkItems.FindAll())
            {
                using var shout = await _serviceMediator.TryExportShoutAsync(barkItem.Signature, Channel, barkItem.ShoutUpdatedTime, cancellationToken);
                if (shout is null) continue;

                var rootHash = RocketMessage.FromBytes<OmniHash>(shout.Value.Memory);

                var newBarkItem = barkItem with
                {
                    RootHash = rootHash,
                    ShoutUpdatedTime = shout.UpdatedTime.ToDateTime(),
                };
                _barkSubscriberRepo.BarkItems.Upsert(newBarkItem);
            }
        }
    }

    private async ValueTask SyncSubscribedFiles(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _serviceMediator.GetSubscribedFileReportsAsync(cancellationToken);
            var rootHashes = reports
                .Where(n => n.Authors.Contains(Author))
                .Select(n => n.RootHash)
                .ToHashSet();

            foreach (var rootHash in rootHashes)
            {
                if (_barkSubscriberRepo.BarkItems.Exists(rootHash)) continue;
                await _serviceMediator.UnpublishFileFromMemoryAsync(rootHash, Author, cancellationToken);
            }

            foreach (var rootHash in _barkSubscriberRepo.BarkItems.FindAll().Select(n => n.RootHash))
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

            var barkItem = _barkSubscriberRepo.BarkItems.FindOne(signature);
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

    public async ValueTask<BarkMessageReport?> FindMessagesBySelfHashAsync(OmniHash selfHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var result = _cachedBarkMessageRepo.FetchMessageBySelfHash(selfHash);
            return result?.ToReport();
        }
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
                    maxBarkMessageCount: 10000
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
