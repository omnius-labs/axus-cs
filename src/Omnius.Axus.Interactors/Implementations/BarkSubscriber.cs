using Omnius.Axus.Interactors.Internal.Repositories;
using Omnius.Axus.Interactors.Models;
using Omnius.Core;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;

namespace Omnius.Axus.Interactors;

public sealed partial class BarkSubscriber : AsyncDisposableBase, IBarkSubscriber
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IAxusServiceMediator _serviceController;
    private readonly IBytesPool _bytesPool;
    private readonly BarkSubscriberOptions _options;

    private readonly BarkSubscriberRepository _barkSubscriberRepo;
    private readonly ISingleValueStorage _configStorage;
    private readonly IKeyValueStorage<string> _cachedBarkStorage;

    private Task _watchLoopTask = null!;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const string Registrant = "Omnius.Axus.Interactors.BarkSubscriber";

    public static async ValueTask<BarkSubscriber> CreateAsync(IAxusServiceMediator service, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, BarkSubscriberOptions options, CancellationToken cancellationToken = default)
    {
        var barkSubscriber = new BarkSubscriber(service, singleValueStorageFactory, keyValueStorageFactory, bytesPool, options);
        await barkSubscriber.InitAsync(cancellationToken);
        return barkSubscriber;
    }

    private BarkSubscriber(IAxusServiceMediator service, ISingleValueStorageFactory singleValueStorageFactory, IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, BarkSubscriberOptions options)
    {
        _serviceController = service;
        _bytesPool = bytesPool;
        _options = options;

        _barkSubscriberRepo = new BarkSubscriberRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
        _configStorage = singleValueStorageFactory.Create(Path.Combine(_options.ConfigDirectoryPath, "config"), _bytesPool);
        _cachedBarkStorage = keyValueStorageFactory.Create<string>(Path.Combine(_options.ConfigDirectoryPath, "profiles"), _bytesPool);
    }

    internal async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        await _barkSubscriberRepo.MigrateAsync(cancellationToken);
        await _cachedBarkStorage.MigrateAsync(cancellationToken);

        _watchLoopTask = this.WatchLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await _watchLoopTask;
        _cancellationTokenSource.Dispose();

        _barkSubscriberRepo.Dispose();
        _configStorage.Dispose();
        _cachedBarkStorage.Dispose();
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
            var reports = await _serviceController.GetSubscribedShoutReportsAsync(cancellationToken);
            var signatures = reports
                .Where(n => n.Registrant == Registrant)
                .Select(n => n.Signature)
                .ToHashSet();

            foreach (var signature in signatures)
            {
                if (_barkSubscriberRepo.Metadatas..(signature)) continue;
                await _serviceController.UnsubscribeShoutAsync(signature, Registrant, cancellationToken);
            }

            foreach (var item in _profileSubscriberRepo.Items.FindAll())
            {
                if (signatures.Contains(item.Signature)) continue;
                await _serviceController.SubscribeShoutAsync(item.Signature, Registrant, cancellationToken);
            }

            foreach (var item in _profileSubscriberRepo.Items.FindAll())
            {
                using var shout = await _serviceController.TryExportShoutAsync(item.Signature, cancellationToken);
                if (shout is null) continue;

                var rootHash = RocketMessage.FromBytes<OmniHash>(shout.Value.Memory);

                var newItem = new SubscribedProfileItem(item.Signature, rootHash, shout.CreatedTime.ToDateTime());
                _profileSubscriberRepo.Items.Upsert(newItem);
            }
        }
    }

    public ValueTask<IEnumerable<BarkMessage>> FindByTagAsync(string tag, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public ValueTask<BarkMessage?> FindByReplyAsync(string tag, BarkReply reply, CancellationToken cancellationToken = default) => throw new NotImplementedException();

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
