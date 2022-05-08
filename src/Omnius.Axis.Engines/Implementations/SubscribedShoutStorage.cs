using System.Buffers;
using Omnius.Axis.Engines.Internal;
using Omnius.Axis.Engines.Internal.Models;
using Omnius.Axis.Engines.Internal.Repositories;
using Omnius.Axis.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Pipelines;
using Omnius.Core.Storages;

namespace Omnius.Axis.Engines;

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
                shoutReports.Add(new SubscribedShoutReport(item.Signature, item.Registrant));
            }

            return shoutReports.ToArray();
        }
    }

    public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
    {
    }

    public async ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var results = new List<OmniSignature>();

            foreach (var item in _subscriberRepo.Items.FindAll())
            {
                results.Add(item.Signature);
            }

            return results;
        }
    }

    public async ValueTask<bool> ContainsShoutAsync(OmniSignature signature, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            if (_subscriberRepo.Items.Exists(signature)) return false;

            return true;
        }
    }

    public async ValueTask SubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            _subscriberRepo.Items.Insert(new SubscribedShoutItem(signature, registrant));
        }
    }

    public async ValueTask UnsubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            _subscriberRepo.Items.Delete(signature, registrant);
        }
    }

    public async ValueTask<DateTime?> ReadShoutCreatedTimeAsync(OmniSignature signature, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var writtenItem = _subscriberRepo.WrittenItems.FindOne(signature);
            if (writtenItem == null) return null;

            return writtenItem.CreatedTime;
        }
    }

    public async ValueTask<Shout?> ReadShoutAsync(OmniSignature signature, CancellationToken cancellationToken = default)
    {
        var item = _subscriberRepo.Items.Find(signature).FirstOrDefault();
        if (item is null) return null;

        var writtenItem = _subscriberRepo.WrittenItems.FindOne(signature);
        if (writtenItem is null) return null;

        var blockName = ComputeBlockName(signature);
        using var memoryOwner = await _blockStorage.TryReadAsync(blockName, cancellationToken);
        if (memoryOwner is null) return null;

        var message = Shout.Import(new ReadOnlySequence<byte>(memoryOwner.Memory), _bytesPool);
        return message;
    }

    public async ValueTask WriteShoutAsync(Shout message, CancellationToken cancellationToken = default)
    {
        if (!message.Verify()) return;

        var signature = message.Certificate?.GetOmniSignature();
        if (signature == null) return;

        if (!_subscriberRepo.Items.Exists(signature)) return;

        var writtenItem = _subscriberRepo.WrittenItems.FindOne(signature);
        if (writtenItem is not null && writtenItem.CreatedTime >= message.CreatedTime.ToDateTime()) return;

        using var bytesPise = new BytesPipe(_bytesPool);
        message.Export(bytesPise.Writer, _bytesPool);

        _subscriberRepo.WrittenItems.Insert(new WrittenShoutItem(signature, message.CreatedTime.ToDateTime()));

        var blockName = ComputeBlockName(signature);
        await _blockStorage.TryWriteAsync(blockName, bytesPise.Reader.GetSequence(), cancellationToken);
    }

    private static string ComputeBlockName(OmniSignature signature)
    {
        return StringConverter.SignatureToString(signature);
    }
}
