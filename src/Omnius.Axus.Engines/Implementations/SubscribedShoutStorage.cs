using System.Buffers;
using Omnius.Axus.Engines.Internal;
using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Engines.Internal.Repositories;
using Omnius.Axus.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Pipelines;
using Omnius.Core.Storages;

namespace Omnius.Axus.Engines;

// TODO
// WrittenItemsは本体に統合する
public sealed partial class SubscribedShoutStorage : AsyncDisposableBase, ISubscribedShoutStorage
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IKeyValueStorageFactory _keyValueStorageFactory;
    private readonly IBytesPool _bytesPool;
    private readonly SubscribedShoutStorageOptions _options;

    private readonly SubscribedShoutStorageRepository _subscriberRepo;
    private readonly IKeyValueStorage<string> _blockStorage;

    private readonly AsyncLock _asyncLock = new();

    public static async ValueTask<SubscribedShoutStorage> CreateAsync(IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, SubscribedShoutStorageOptions options, CancellationToken cancellationToken = default)
    {
        var subscribedShoutStorage = new SubscribedShoutStorage(keyValueStorageFactory, bytesPool, options);
        await subscribedShoutStorage.InitAsync(cancellationToken);
        return subscribedShoutStorage;
    }

    private SubscribedShoutStorage(IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, SubscribedShoutStorageOptions options)
    {
        _keyValueStorageFactory = keyValueStorageFactory;
        _bytesPool = bytesPool;
        _options = options;

        _subscriberRepo = new SubscribedShoutStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
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

    public async ValueTask<IEnumerable<SubscribedShoutReport>> GetSubscribedShoutReportsAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shoutReports = new List<SubscribedShoutReport>();

            foreach (var item in _subscriberRepo.Items.FindAll())
            {
                shoutReports.Add(new SubscribedShoutReport(item.Signature, item.Channel, item.Registrant));
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

            foreach (var item in _subscriberRepo.Items.FindAll())
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
            return _subscriberRepo.Items.Exists(signature, channel);
        }
    }

    public async ValueTask SubscribeShoutAsync(OmniSignature signature, string channel, string registrant, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            _subscriberRepo.Items.Insert(new SubscribedShoutItem(signature, channel, registrant));
        }
    }

    public async ValueTask UnsubscribeShoutAsync(OmniSignature signature, string channel, string registrant, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            _subscriberRepo.Items.Delete(signature, channel, registrant);

            // TODO: _blockStorageからの削除
        }
    }

    public async ValueTask<DateTime?> ReadShoutCreatedTimeAsync(OmniSignature signature, string channel, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var writtenItem = _subscriberRepo.WrittenItems.FindOne(signature, channel);
            if (writtenItem == null) return null;

            return writtenItem.CreatedTime.ToUniversalTime();
        }
    }

    public async ValueTask<Shout?> ReadShoutAsync(OmniSignature signature, string channel, CancellationToken cancellationToken = default)
    {
        var item = _subscriberRepo.Items.Find(signature, channel).FirstOrDefault();
        if (item is null) return null;

        var writtenItem = _subscriberRepo.WrittenItems.FindOne(signature, channel);
        if (writtenItem is null) return null;

        var blockName = ComputeBlockName(signature, channel);
        using var memoryOwner = await _blockStorage.TryReadAsync(blockName, cancellationToken);
        if (memoryOwner is null) return null;

        var message = Shout.Import(new ReadOnlySequence<byte>(memoryOwner.Memory), _bytesPool);
        return message;
    }

    public async ValueTask WriteShoutAsync(Shout shout, CancellationToken cancellationToken = default)
    {
        if (!shout.Verify()) return;

        var signature = shout.Certificate?.GetOmniSignature();
        if (signature == null) return;

        if (!_subscriberRepo.Items.Exists(signature, shout.Channel)) return;

        var writtenItem = _subscriberRepo.WrittenItems.FindOne(signature, shout.Channel);
        if (writtenItem is not null && writtenItem.CreatedTime >= shout.CreatedTime.ToDateTime()) return;

        using var bytesPipe = new BytesPipe(_bytesPool);
        shout.Export(bytesPipe.Writer, _bytesPool);

        _subscriberRepo.WrittenItems.Insert(new WrittenShoutItem(signature, shout.Channel, shout.CreatedTime.ToDateTime()));

        var blockName = ComputeBlockName(signature, shout.Channel);
        await _blockStorage.WriteAsync(blockName, bytesPipe.Reader.GetSequence(), cancellationToken);
    }

    private static string ComputeBlockName(OmniSignature signature, string channel)
    {
        return StringConverter.ToString(signature, channel);
    }
}
