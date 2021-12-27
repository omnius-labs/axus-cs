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

public sealed partial class PublishedShoutStorage : AsyncDisposableBase, IPublishedShoutStorage
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IKeyValueStorageFactory _keyValueStorageFactory;
    private readonly IBytesPool _bytesPool;
    private readonly PublishedShoutStorageOptions _options;

    private readonly PublishedShoutStorageRepository _publisherRepo;
    private readonly IKeyValueStorage<string> _blockStorage;

    private readonly AsyncLock _asyncLock = new();

    public static async ValueTask<PublishedShoutStorage> CreateAsync(IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, PublishedShoutStorageOptions options, CancellationToken cancellationToken = default)
    {
        var publishedShoutStorage = new PublishedShoutStorage(keyValueStorageFactory, bytesPool, options);
        await publishedShoutStorage.InitAsync(cancellationToken);
        return publishedShoutStorage;
    }

    private PublishedShoutStorage(IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, PublishedShoutStorageOptions options)
    {
        _keyValueStorageFactory = keyValueStorageFactory;
        _bytesPool = bytesPool;
        _options = options;

        _publisherRepo = new PublishedShoutStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "state"));
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

    public async ValueTask<IEnumerable<PublishedShoutReport>> GetPublishedShoutReportsAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shoutReports = new List<PublishedShoutReport>();

            foreach (var item in _publisherRepo.Items.FindAll())
            {
                shoutReports.Add(new PublishedShoutReport(item.Signature, item.Registrant));
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

            foreach (var status in _publisherRepo.Items.FindAll())
            {
                results.Add(status.Signature);
            }

            return results;
        }
    }

    public async ValueTask<bool> ContainsShoutAsync(OmniSignature signature, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var item = _publisherRepo.Items.Find(signature).FirstOrDefault();
            if (item == null) return false;

            return true;
        }
    }

    public async ValueTask PublishShoutAsync(Shout message, string registrant, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var signature = message.Certificate?.GetOmniSignature();
            if (signature is null) throw new ArgumentNullException(nameof(message.Certificate));

            using var bytesPipe = new BytesPipe(_bytesPool);
            message.Export(bytesPipe.Writer, _bytesPool);

            _publisherRepo.Items.Insert(new PublishedShoutItem(signature, message.CreationTime.ToDateTime(), registrant));

            var blockName = ComputeBlockName(signature);
            await _blockStorage.TryWriteAsync(blockName, bytesPipe.Reader.GetSequence(), cancellationToken);
        }
    }

    public async ValueTask UnpublishShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            _publisherRepo.Items.Delete(signature, registrant);
        }
    }

    public async ValueTask<DateTime?> ReadShoutCreationTimeAsync(OmniSignature signature, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var item = _publisherRepo.Items.Find(signature).FirstOrDefault();
            if (item == null) return null;

            return item.CreationTime;
        }
    }

    public async ValueTask<Shout?> ReadShoutAsync(OmniSignature signature, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var item = _publisherRepo.Items.Find(signature).FirstOrDefault();
            if (item == null) return null;

            var blockName = ComputeBlockName(signature);
            var memoryOwner = await _blockStorage.TryReadAsync(blockName, cancellationToken);
            if (memoryOwner is null) return null;

            var message = Shout.Import(new ReadOnlySequence<byte>(memoryOwner.Memory), _bytesPool);
            return message;
        }
    }

    private static string ComputeBlockName(OmniSignature signature)
    {
        return StringConverter.SignatureToString(signature);
    }
}
