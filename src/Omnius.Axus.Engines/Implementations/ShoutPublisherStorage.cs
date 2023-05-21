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

public sealed partial class ShoutPublisherStorage : AsyncDisposableBase, IShoutPublisherStorage
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IKeyValueStorageFactory _keyValueStorageFactory;
    private readonly IBytesPool _bytesPool;
    private readonly ShoutPublisherStorageOptions _options;

    private readonly ShoutPublisherStorageRepository _publisherRepo;
    private readonly IKeyValueStorage<string> _blockStorage;

    private readonly AsyncLock _asyncLock = new();

    public static async ValueTask<ShoutPublisherStorage> CreateAsync(IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, ShoutPublisherStorageOptions options, CancellationToken cancellationToken = default)
    {
        var publishedShoutStorage = new ShoutPublisherStorage(keyValueStorageFactory, bytesPool, options);
        await publishedShoutStorage.InitAsync(cancellationToken);
        return publishedShoutStorage;
    }

    private ShoutPublisherStorage(IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, ShoutPublisherStorageOptions options)
    {
        _keyValueStorageFactory = keyValueStorageFactory;
        _bytesPool = bytesPool;
        _options = options;

        _publisherRepo = new ShoutPublisherStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
        _blockStorage = _keyValueStorageFactory.Create<string>(Path.Combine(_options.ConfigDirectoryPath, "blocks"), _bytesPool);
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        await _publisherRepo.MigrateAsync(cancellationToken);
        await _blockStorage.MigrateAsync(cancellationToken);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _publisherRepo.Dispose();
        _blockStorage.Dispose();
    }

    public async ValueTask<IEnumerable<PublishedShoutReport>> GetPublishedShoutReportsAsync(string zone, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shoutReports = new List<PublishedShoutReport>();

            foreach (var item in _publisherRepo.ShoutItems.Find(zone))
            {
                shoutReports.Add(new PublishedShoutReport(item.Signature, item.Channel, Timestamp64.FromDateTime(item.CreatedTime), item.Properties.ToArray()));
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

            foreach (var status in _publisherRepo.ShoutItems.FindAll())
            {
                results.Add((status.Signature, status.Channel));
            }

            return results;
        }
    }

    public async ValueTask<bool> ContainsShoutAsync(OmniSignature signature, string channel, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            return _publisherRepo.ShoutItems.Exists(signature, channel);
        }
    }

    public async ValueTask PublishShoutAsync(Shout shout, IEnumerable<AttachedProperty> properties, string zone, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var signature = shout.Certificate?.GetOmniSignature();
            if (signature is null) throw new ArgumentNullException(nameof(shout.Certificate));

            using var bytesPipe = new BytesPipe(_bytesPool);
            shout.Export(bytesPipe.Writer, _bytesPool);

            var newShoutItem = new ShoutPublishedItem()
            {
                Signature = signature,
                Properties = properties.ToArray(),
                Channel = shout.Channel,
                CreatedTime = shout.CreatedTime.ToDateTime(),
                Zones = new[] { zone }
            };
            _publisherRepo.ShoutItems.Upsert(newShoutItem);

            var key = GenKey(signature, shout.Channel);
            await _blockStorage.WriteAsync(key, bytesPipe.Reader.GetSequence(), cancellationToken);
        }
    }

    public async ValueTask UnpublishShoutAsync(OmniSignature signature, string channel, string zone, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            _publisherRepo.ShoutItems.Delete(signature, channel);

            var key = GenKey(signature, channel);
            await _blockStorage.TryDeleteAsync(key, cancellationToken);
        }
    }

    public async ValueTask<DateTime> ReadShoutCreatedTimeAsync(OmniSignature signature, string channel, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shoutItem = _publisherRepo.ShoutItems.FindOne(signature, channel);
            if (shoutItem is null) return DateTime.MinValue;

            return shoutItem.CreatedTime;
        }
    }

    public async ValueTask<Shout?> TryReadShoutAsync(OmniSignature signature, string channel, DateTime updatedTime, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shoutItem = _publisherRepo.ShoutItems.FindOne(signature, channel);
            if (shoutItem is null || shoutItem.CreatedTime <= updatedTime) return null;

            var key = GenKey(signature, channel);
            using var memoryOwner = await _blockStorage.TryReadAsync(key, cancellationToken);
            if (memoryOwner is null) return null;

            var message = Shout.Import(new ReadOnlySequence<byte>(memoryOwner.Memory), _bytesPool);
            return message;
        }
    }

    private static string GenKey(OmniSignature signature, string channel)
    {
        return StringConverter.ToString(signature, channel);
    }
}
