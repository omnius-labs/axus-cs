using System.Buffers;
using Omnius.Axus.Engine.Internal.Models;
using Omnius.Axus.Engine.Internal.Repositories;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Pipelines;
using Omnius.Core.RocketPack;
using Omnius.Core.Storages;

namespace Omnius.Axus.Engine.Internal.Services;

public sealed partial class ShoutPublisherStorage : AsyncDisposableBase, IShoutPublisherStorage
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IKeyValueStorageFactory _keyValueStorageFactory;
    private readonly ISystemClock _systemClock;
    private readonly IBytesPool _bytesPool;
    private readonly ShoutPublisherStorageOptions _options;

    private readonly ShoutPublisherStorageRepository _publisherRepo;
    private readonly IKeyValueStorage _blockStorage;

    private readonly AsyncLock _asyncLock = new();

    public static async ValueTask<ShoutPublisherStorage> CreateAsync(IKeyValueStorageFactory keyValueStorageFactory,
        ISystemClock systemClock, IBytesPool bytesPool, ShoutPublisherStorageOptions options, CancellationToken cancellationToken = default)
    {
        var publishedShoutStorage = new ShoutPublisherStorage(keyValueStorageFactory, systemClock, bytesPool, options);
        await publishedShoutStorage.InitAsync(cancellationToken);
        return publishedShoutStorage;
    }

    private ShoutPublisherStorage(IKeyValueStorageFactory keyValueStorageFactory,
        ISystemClock systemClock, IBytesPool bytesPool, ShoutPublisherStorageOptions options)
    {
        _keyValueStorageFactory = keyValueStorageFactory;
        _systemClock = systemClock;
        _bytesPool = bytesPool;
        _options = options;

        _publisherRepo = new ShoutPublisherStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "status"), _bytesPool);
        _blockStorage = _keyValueStorageFactory.Create(Path.Combine(_options.ConfigDirectoryPath, "blocks"), _bytesPool);
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        await _publisherRepo.MigrateAsync(cancellationToken);
        await _blockStorage.MigrateAsync(cancellationToken);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        await _publisherRepo.DisposeAsync();
        await _blockStorage.DisposeAsync();
    }

    public async ValueTask<IEnumerable<PublishedShoutReport>> GetPublishedShoutReportsAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shoutReports = new List<PublishedShoutReport>();

            await foreach (var item in _publisherRepo.ShoutItems.GetItemsAsync(cancellationToken))
            {
                shoutReports.Add(new PublishedShoutReport(item.Signature, item.Channel, Timestamp64.FromDateTime(item.ShoutCreatedTime), item.Property));
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

            await foreach (var status in _publisherRepo.ShoutItems.GetItemsAsync(cancellationToken))
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
            return await _publisherRepo.ShoutItems.ExistsAsync(signature, channel, cancellationToken);
        }
    }

    public async ValueTask PublishShoutAsync(Shout shout, AttachedProperty? property, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var signature = shout.Certificate?.GetOmniSignature();
            if (signature is null) throw new ArgumentNullException(nameof(shout.Certificate));

            var now = _systemClock.GetUtcNow();

            using var bytesPipe = new BytesPipe(_bytesPool);
            shout.Export(bytesPipe.Writer, _bytesPool);

            var newShoutItem = new ShoutPublishedItem()
            {
                Signature = signature,
                Property = property,
                Channel = shout.Channel,
                ShoutCreatedTime = shout.CreatedTime.ToDateTime(),
                CreatedTime = now,
                UpdatedTime = now,
            };
            await _publisherRepo.ShoutItems.UpsertAsync(newShoutItem, cancellationToken);

            var key = GenKey(signature, shout.Channel);
            await _blockStorage.WriteAsync(key, bytesPipe.Reader.GetSequence(), cancellationToken);
        }
    }

    public async ValueTask UnpublishShoutAsync(OmniSignature signature, string channel, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _publisherRepo.ShoutItems.DeleteAsync(signature, channel, cancellationToken);

            var key = GenKey(signature, channel);
            await _blockStorage.TryDeleteAsync(key, cancellationToken);
        }
    }

    public async ValueTask<DateTime> ReadShoutCreatedTimeAsync(OmniSignature signature, string channel, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shoutItem = await _publisherRepo.ShoutItems.GetItemAsync(signature, channel, cancellationToken);
            if (shoutItem is null) return DateTime.MinValue.ToUniversalTime();

            return shoutItem.ShoutCreatedTime;
        }
    }

    public async ValueTask<Shout?> TryReadShoutAsync(OmniSignature signature, string channel, DateTime createdTime, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shoutItem = await _publisherRepo.ShoutItems.GetItemAsync(signature, channel, cancellationToken);
            if (shoutItem is null || shoutItem.ShoutCreatedTime <= createdTime) return null;

            var key = GenKey(signature, channel);
            using var memoryOwner = await _blockStorage.TryReadAsync(key, cancellationToken);
            if (memoryOwner is null) return null;

            var message = Shout.Import(new ReadOnlySequence<byte>(memoryOwner.Memory), _bytesPool);
            return message;
        }
    }

    private static string GenKey(OmniSignature signature, string channel)
    {
        return signature.ToString() + "/" + channel;
    }
}
