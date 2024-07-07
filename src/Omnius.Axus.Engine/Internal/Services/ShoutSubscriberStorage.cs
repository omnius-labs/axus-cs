using System.Buffers;
using Omnius.Axus.Engine.Internal;
using Omnius.Axus.Engine.Internal.Models;
using Omnius.Axus.Engine.Internal.Repositories;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Pipelines;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;

namespace Omnius.Axus.Engine.Internal.Services;

public sealed partial class ShoutSubscriberStorage : AsyncDisposableBase, IShoutSubscriberStorage
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IKeyValueStorageFactory _keyValueStorageFactory;
    private readonly ISystemClock _systemClock;
    private readonly IBytesPool _bytesPool;
    private readonly ShoutSubscriberStorageOptions _options;

    private readonly ShoutSubscriberStorageRepository _subscriberRepo;
    private readonly IKeyValueStorage _blockStorage;

    private readonly AsyncLock _asyncLock = new();

    public static async ValueTask<ShoutSubscriberStorage> CreateAsync(IKeyValueStorageFactory keyValueStorageFactory,
        ISystemClock systemClock, IBytesPool bytesPool, ShoutSubscriberStorageOptions options, CancellationToken cancellationToken = default)
    {
        var subscribedShoutStorage = new ShoutSubscriberStorage(keyValueStorageFactory, systemClock, bytesPool, options);
        await subscribedShoutStorage.InitAsync(cancellationToken);
        return subscribedShoutStorage;
    }

    private ShoutSubscriberStorage(IKeyValueStorageFactory keyValueStorageFactory,
        ISystemClock systemClock, IBytesPool bytesPool, ShoutSubscriberStorageOptions options)
    {
        _keyValueStorageFactory = keyValueStorageFactory;
        _systemClock = systemClock;
        _bytesPool = bytesPool;
        _options = options;

        _subscriberRepo = new ShoutSubscriberStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "status"), _bytesPool);
        _blockStorage = _keyValueStorageFactory.Create(Path.Combine(_options.ConfigDirectoryPath, "blocks"), _bytesPool);
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        await _subscriberRepo.MigrateAsync(cancellationToken);
        await _blockStorage.MigrateAsync(cancellationToken);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        await _subscriberRepo.DisposeAsync();
        await _blockStorage.DisposeAsync();
    }

    public async ValueTask<IEnumerable<SubscribedShoutReport>> GetSubscribedShoutReportsAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shoutReports = new List<SubscribedShoutReport>();

            await foreach (var item in _subscriberRepo.ShoutItems.GetItemsAsync(cancellationToken))
            {
                shoutReports.Add(new SubscribedShoutReport(item.Signature, item.Channel, Timestamp64.FromDateTime(item.ShoutCreatedTime), item.Property));
            }

            return shoutReports.ToArray();
        }
    }

    public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
    {
    }

    public async ValueTask<IEnumerable<(OmniSignature, string)>> GetKeysAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var results = new List<(OmniSignature, string)>();

            await foreach (var item in _subscriberRepo.ShoutItems.GetItemsAsync(cancellationToken))
            {
                results.Add((item.Signature, item.Channel));
            }

            return results;
        }
    }

    public async ValueTask<bool> ContainsShoutAsync(OmniSignature signature, string channel, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            return await _subscriberRepo.ShoutItems.ExistsAsync(signature, channel);
        }
    }

    public async ValueTask SubscribeShoutAsync(OmniSignature signature, AttachedProperty? property, string channel, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shoutItem = await _subscriberRepo.ShoutItems.GetItemAsync(signature, channel, cancellationToken);
            if (shoutItem is not null) return;

            var now = _systemClock.GetUtcNow();

            var newShoutItem = new ShoutSubscribedItem()
            {
                Signature = signature,
                Property = property,
                Channel = channel,
                ShoutCreatedTime = DateTime.MinValue.ToUniversalTime(),
                CreatedTime = now,
                UpdatedTime = now,
            };
            await _subscriberRepo.ShoutItems.UpsertAsync(newShoutItem, cancellationToken);
        }
    }

    public async ValueTask UnsubscribeShoutAsync(OmniSignature signature, string channel, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shoutItem = await _subscriberRepo.ShoutItems.GetItemAsync(signature, channel, cancellationToken);
            if (shoutItem is null) return;

            await _subscriberRepo.ShoutItems.DeleteAsync(signature, channel, cancellationToken);

            var key = GenKey(signature, channel);
            await _blockStorage.TryDeleteAsync(key, cancellationToken);
        }
    }

    public async ValueTask<DateTime> ReadShoutCreatedTimeAsync(OmniSignature signature, string channel, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shoutItem = await _subscriberRepo.ShoutItems.GetItemAsync(signature, channel, cancellationToken);
            if (shoutItem is null) return DateTime.MinValue.ToUniversalTime();

            return shoutItem.ShoutCreatedTime;
        }
    }

    public async ValueTask<Shout?> TryReadShoutAsync(OmniSignature signature, string channel, DateTime createdTime, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shoutItem = await _subscriberRepo.ShoutItems.GetItemAsync(signature, channel, cancellationToken);
            if (shoutItem is null || shoutItem.ShoutCreatedTime <= createdTime) return null;

            var blockName = GenKey(signature, channel);
            using var memoryOwner = await _blockStorage.TryReadAsync(blockName, cancellationToken);
            if (memoryOwner is null) return null;

            var message = Shout.Import(new ReadOnlySequence<byte>(memoryOwner.Memory), _bytesPool);
            return message;
        }
    }

    public async ValueTask WriteShoutAsync(Shout shout, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            if (!shout.Verify()) return;

            var signature = shout.Certificate?.GetOmniSignature();
            if (signature == null) return;

            var shoutItem = await _subscriberRepo.ShoutItems.GetItemAsync(signature, shout.Channel, cancellationToken);
            if (shoutItem is null || shoutItem.ShoutCreatedTime >= shout.CreatedTime.ToDateTime()) return;

            var newShoutItem = shoutItem with { ShoutCreatedTime = shout.CreatedTime.ToDateTime() };
            await _subscriberRepo.ShoutItems.UpsertAsync(newShoutItem, cancellationToken);

            using var bytesPipe = new BytesPipe(_bytesPool);
            shout.Export(bytesPipe.Writer, _bytesPool);

            var key = GenKey(signature, shout.Channel);
            await _blockStorage.WriteAsync(key, bytesPipe.Reader.GetSequence(), cancellationToken);
        }
    }

    private static string GenKey(OmniSignature signature, string channel)
    {
        return signature.ToString() + "/" + channel;
    }
}
