using System.Buffers;
using Omnius.Axus.Engines.Internal;
using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Engines.Internal.Repositories;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Pipelines;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;

namespace Omnius.Axus.Engines;

public sealed partial class ShoutSubscriberStorage : AsyncDisposableBase, IShoutSubscriberStorage
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IKeyValueStorageFactory _keyValueStorageFactory;
    private readonly IBytesPool _bytesPool;
    private readonly ShoutSubscriberStorageOptions _options;

    private readonly ShoutSubscriberStorageRepository _subscriberRepo;
    private readonly IKeyValueStorage<string> _blockStorage;

    private readonly AsyncLock _asyncLock = new();

    public static async ValueTask<ShoutSubscriberStorage> CreateAsync(IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, ShoutSubscriberStorageOptions options, CancellationToken cancellationToken = default)
    {
        var subscribedShoutStorage = new ShoutSubscriberStorage(keyValueStorageFactory, bytesPool, options);
        await subscribedShoutStorage.InitAsync(cancellationToken);
        return subscribedShoutStorage;
    }

    private ShoutSubscriberStorage(IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, ShoutSubscriberStorageOptions options)
    {
        _keyValueStorageFactory = keyValueStorageFactory;
        _bytesPool = bytesPool;
        _options = options;

        _subscriberRepo = new ShoutSubscriberStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
        _blockStorage = _keyValueStorageFactory.Create<string>(Path.Combine(_options.ConfigDirectoryPath, "blocks"), _bytesPool);
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        await _subscriberRepo.MigrateAsync(cancellationToken);
        await _blockStorage.MigrateAsync(cancellationToken);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _subscriberRepo.Dispose();
        _blockStorage.Dispose();
    }

    public async ValueTask<IEnumerable<SubscribedShoutReport>> GetSubscribedShoutReportsAsync(string zone, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shoutReports = new List<SubscribedShoutReport>();

            foreach (var item in _subscriberRepo.ShoutItems.Find(zone))
            {
                shoutReports.Add(new SubscribedShoutReport(item.Signature, item.Channel, Timestamp64.FromDateTime(item.CreatedTime), item.Properties.ToArray()));
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

            foreach (var item in _subscriberRepo.ShoutItems.FindAll())
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
            return _subscriberRepo.ShoutItems.Exists(signature, channel);
        }
    }

    public async ValueTask SubscribeShoutAsync(OmniSignature signature, IEnumerable<AttachedProperty> properties, string channel, string zone, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shoutItem = _subscriberRepo.ShoutItems.FindOne(signature, channel);

            if (shoutItem is not null)
            {
                if (shoutItem.Zones.Contains(zone)) return;

                var zones = shoutItem.Zones.Append(zone).ToArray();
                shoutItem = shoutItem with { Zones = zones };
                _subscriberRepo.ShoutItems.Upsert(shoutItem);

                return;
            }

            var newShoutItem = new ShoutSubscribedItem()
            {
                Signature = signature,
                Properties = properties.ToArray(),
                Channel = channel,
                CreatedTime = DateTime.MinValue,
                Zones = new[] { zone }
            };
            _subscriberRepo.ShoutItems.Upsert(newShoutItem);
        }
    }

    public async ValueTask UnsubscribeShoutAsync(OmniSignature signature, string channel, string zone, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shoutItem = _subscriberRepo.ShoutItems.FindOne(signature, channel);
            if (shoutItem is null) return;
            if (!shoutItem.Zones.Contains(zone)) return;

            if (shoutItem.Zones.Count > 1)
            {
                var zones = shoutItem.Zones.Where(n => n != zone).ToArray();
                var newShoutItem = shoutItem with { Zones = zones };
                _subscriberRepo.ShoutItems.Upsert(newShoutItem);

                return;
            }

            _subscriberRepo.ShoutItems.Delete(signature, channel);

            var key = GenKey(signature, channel);
            await _blockStorage.TryDeleteAsync(key, cancellationToken);
        }
    }

    public async ValueTask<DateTime> ReadShoutCreatedTimeAsync(OmniSignature signature, string channel, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shoutItem = _subscriberRepo.ShoutItems.FindOne(signature, channel);
            if (shoutItem is null) return DateTime.MinValue;

            return shoutItem.CreatedTime;
        }
    }

    public async ValueTask<Shout?> TryReadShoutAsync(OmniSignature signature, string channel, DateTime updatedTime, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shoutItem = _subscriberRepo.ShoutItems.FindOne(signature, channel);
            if (shoutItem is null || shoutItem.CreatedTime <= updatedTime) return null;

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

            var shoutItem = _subscriberRepo.ShoutItems.FindOne(signature, shout.Channel);
            if (shoutItem is null || shoutItem.CreatedTime >= shout.CreatedTime.ToDateTime()) return;

            var newShoutItem = shoutItem with { CreatedTime = shout.CreatedTime.ToDateTime() };
            _subscriberRepo.ShoutItems.Upsert(newShoutItem);

            using var bytesPipe = new BytesPipe(_bytesPool);
            shout.Export(bytesPipe.Writer, _bytesPool);

            var key = GenKey(signature, shout.Channel);
            await _blockStorage.WriteAsync(key, bytesPipe.Reader.GetSequence(), cancellationToken);
        }
    }

    private static string GenKey(OmniSignature signature, string channel)
    {
        return StringConverter.ToString(signature, channel);
    }
}
