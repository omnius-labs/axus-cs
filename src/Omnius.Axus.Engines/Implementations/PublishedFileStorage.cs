using System.Buffers;
using Omnius.Axus.Engines.Internal;
using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Engines.Internal.Repositories;
using Omnius.Axus.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Cryptography.Functions;
using Omnius.Core.Pipelines;
using Omnius.Core.Storages;

namespace Omnius.Axus.Engines;

public sealed partial class PublishedFileStorage : AsyncDisposableBase, IPublishedFileStorage
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IKeyValueStorageFactory _keyValueStorageFactory;
    private readonly IBytesPool _bytesPool;
    private readonly PublishedFileStorageOptions _options;

    private readonly PublishedFileStorageRepository _publisherRepo;
    private readonly IKeyValueStorage<string> _blockStorage;

    private readonly AsyncLock _asyncLock = new();
    private readonly AsyncLock _publishAsyncLock = new();

    private const int MaxBlockLength = 256 * 1024;

    public static async ValueTask<PublishedFileStorage> CreateAsync(IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, PublishedFileStorageOptions options, CancellationToken cancellationToken = default)
    {
        var publishedFileStorage = new PublishedFileStorage(keyValueStorageFactory, bytesPool, options);
        await publishedFileStorage.InitAsync(cancellationToken);
        return publishedFileStorage;
    }

    private PublishedFileStorage(IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, PublishedFileStorageOptions options)
    {
        _keyValueStorageFactory = keyValueStorageFactory;
        _bytesPool = bytesPool;
        _options = options;

        _publisherRepo = new PublishedFileStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
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

    public async ValueTask<IEnumerable<PublishedFileReport>> GetPublishedFileReportsAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var fileReports = new List<PublishedFileReport>();

            foreach (var item in _publisherRepo.Items.FindAll())
            {
                fileReports.Add(new PublishedFileReport(item.FilePath, item.RootHash, item.Registrant));
            }

            return fileReports.ToArray();
        }
    }

    // TODO 実装する
    public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
    {
    }

    public async ValueTask<IEnumerable<OmniHash>> GetPushRootHashesAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var results = new List<OmniHash>();

            foreach (var item in _publisherRepo.Items.FindAll())
            {
                results.Add(item.RootHash);
            }

            return results;
        }
    }

    public async ValueTask<IEnumerable<OmniHash>> GetPushBlockHashesAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var item = _publisherRepo.Items.Find(rootHash).FirstOrDefault();
            if (item is null) return Enumerable.Empty<OmniHash>();

            return item.MerkleTreeSections.SelectMany(n => n.Hashes).ToArray();
        }
    }

    public async ValueTask<bool> ContainsPushContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            if (!_publisherRepo.Items.Exists(rootHash)) return false;

            return true;
        }
    }

    public async ValueTask<bool> ContainsPushBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var item = _publisherRepo.Items.Find(rootHash).FirstOrDefault();
            if (item is null) return false;

            return item.MerkleTreeSections.Any(n => n.Contains(blockHash));
        }
    }

    public async ValueTask<OmniHash> PublishFileAsync(string filePath, string registrant, CancellationToken cancellationToken = default)
    {
        using (await _publishAsyncLock.LockAsync(cancellationToken))
        {
            PublishedFileItem? item;

            using (await _asyncLock.LockAsync(cancellationToken))
            {
                item = _publisherRepo.Items.FindOne(filePath, registrant);
                if (item is not null) return item.RootHash;
            }

            var tempPrefix = "_temp_" + Guid.NewGuid().ToString("N");

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var blockHashes = await this.ImportStreamAsync(fileStream, MaxBlockLength, cancellationToken);
            var lastMerkleTreeSection = new MerkleTreeSection(0, blockHashes.ToArray());

            var (rootHash, merkleTreeSections) = await this.GenMerkleTreeSectionsAsync(tempPrefix, lastMerkleTreeSection, MaxBlockLength, cancellationToken);
            item = new PublishedFileItem(rootHash, filePath, registrant, merkleTreeSections.ToArray(), MaxBlockLength);

            using (await _asyncLock.LockAsync(cancellationToken))
            {
                var newPrefix = StringConverter.HashToString(rootHash);
                var targetBlockHashes = merkleTreeSections.SkipLast(1).SelectMany(n => n.Hashes).ToArray();
                await this.RenameBlocksAsync(tempPrefix, newPrefix, targetBlockHashes, cancellationToken);

                _publisherRepo.Items.Upsert(item);
            }

            return rootHash;
        }
    }

    public async ValueTask<OmniHash> PublishFileAsync(ReadOnlySequence<byte> sequence, string registrant, CancellationToken cancellationToken = default)
    {
        using (await _publishAsyncLock.LockAsync(cancellationToken))
        {
            var tempPrefix = "_temp_" + Guid.NewGuid().ToString("N");

            var blockHashes = await this.ImportBytesAsync(tempPrefix, sequence, MaxBlockLength, cancellationToken);
            var lastMerkleTreeSection = new MerkleTreeSection(0, blockHashes.ToArray());

            var (rootHash, merkleTreeSections) = await this.GenMerkleTreeSectionsAsync(tempPrefix, lastMerkleTreeSection, MaxBlockLength, cancellationToken);
            var item = new PublishedFileItem(rootHash, null, registrant, merkleTreeSections.ToArray(), MaxBlockLength);

            using (await _asyncLock.LockAsync(cancellationToken))
            {
                var newPrefix = StringConverter.HashToString(rootHash);
                var targetBlockHashes = merkleTreeSections.SelectMany(n => n.Hashes).ToArray();
                await this.RenameBlocksAsync(tempPrefix, newPrefix, targetBlockHashes, cancellationToken);

                _publisherRepo.Items.Upsert(item);
            }

            return rootHash;
        }
    }

    private async ValueTask RenameBlocksAsync(string oldPrefix, string newPrefix, IEnumerable<OmniHash> blockHashes, CancellationToken cancellationToken = default)
    {
        foreach (var blockHash in blockHashes.ToHashSet())
        {
            var oldName = GenKey(oldPrefix, blockHash);
            var newName = GenKey(newPrefix, blockHash);
            await _blockStorage.TryChangeKeyAsync(oldName, newName, cancellationToken);
        }
    }

    private async ValueTask<(OmniHash, IEnumerable<MerkleTreeSection>)> GenMerkleTreeSectionsAsync(string prefix, MerkleTreeSection merkleTreeSection, int maxBlockLength, CancellationToken cancellationToken = default)
    {
        var results = new Stack<MerkleTreeSection>();
        results.Push(merkleTreeSection);

        for (; ; )
        {
            using var bytesPool = new BytesPipe(_bytesPool);
            merkleTreeSection.Export(bytesPool.Writer, _bytesPool);

            var blockHashes = await ImportBytesAsync(prefix, bytesPool.Reader.GetSequence(), maxBlockLength, cancellationToken);
            merkleTreeSection = new MerkleTreeSection(merkleTreeSection.Depth + 1, blockHashes.ToArray());
            results.Push(merkleTreeSection);

            if (merkleTreeSection.Hashes.Count == 1) return (merkleTreeSection.Hashes.Single(), results.ToArray());
        }
    }

    private async ValueTask<IEnumerable<OmniHash>> ImportBytesAsync(string prefix, ReadOnlySequence<byte> sequence, int maxBlockLength, CancellationToken cancellationToken = default)
    {
        var blockHashes = new List<OmniHash>();

        using var memoryOwner = _bytesPool.Memory.Rent(maxBlockLength).Shrink(maxBlockLength);

        while (sequence.Length > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var blockLength = (int)Math.Min(sequence.Length, maxBlockLength);
            var memory = memoryOwner.Memory[..blockLength];
            sequence.CopyTo(memory.Span);

            var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memory.Span));
            blockHashes.Add(hash);

            await this.WriteBlockAsync(prefix, hash, memory);

            sequence = sequence.Slice(blockLength);
        }

        return blockHashes;
    }

    private async ValueTask<IEnumerable<OmniHash>> ImportStreamAsync(Stream stream, int maxBlockLength, CancellationToken cancellationToken = default)
    {
        var blockHashes = new List<OmniHash>();

        using var memoryOwner = _bytesPool.Memory.Rent(maxBlockLength).Shrink(maxBlockLength);

        while (stream.Position < stream.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var blockLength = (int)Math.Min(maxBlockLength, stream.Length - stream.Position);
            var memory = memoryOwner.Memory[..blockLength];

            for (int start = 0; start < blockLength;)
            {
                start += await stream.ReadAsync(memory[start..], cancellationToken);
            }

            var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memory.Span));
            blockHashes.Add(hash);
        }

        return blockHashes;
    }

    private async ValueTask WriteBlockAsync(string prefix, OmniHash blockHash, ReadOnlyMemory<byte> memory)
    {
        var key = GenKey(prefix, blockHash);
        await _blockStorage.WriteAsync(key, memory);
    }

    public async ValueTask UnpublishFileAsync(string filePath, string registrant, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var item = _publisherRepo.Items.FindOne(filePath, registrant);
            if (item == null) return;

            _publisherRepo.Items.Delete(filePath, registrant);

            if (_publisherRepo.Items.Exists(item.RootHash)) return;

            await this.DeleteBlocksAsync(item.RootHash, item.MerkleTreeSections.SelectMany(n => n.Hashes));
        }
    }

    public async ValueTask UnpublishFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var item = _publisherRepo.Items.FindOne(rootHash, registrant);
            if (item == null) return;

            _publisherRepo.Items.Delete(item.RootHash, registrant);

            if (_publisherRepo.Items.Exists(item.RootHash)) return;

            await this.DeleteBlocksAsync(item.RootHash, item.MerkleTreeSections.SelectMany(n => n.Hashes));
        }
    }

    private async ValueTask DeleteBlocksAsync(OmniHash rootHash, IEnumerable<OmniHash> blockHashes)
    {
        foreach (var blockHash in blockHashes)
        {
            var key = GenKey(rootHash, blockHash);
            await _blockStorage.TryDeleteAsync(key);
        }
    }

    public async ValueTask<IMemoryOwner<byte>?> ReadBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var item = _publisherRepo.Items.Find(rootHash).FirstOrDefault();
            if (item is null) return null;

            if (!item.MerkleTreeSections.Any(n => n.Contains(blockHash))) return null;

            if (item.FilePath is not null)
            {
                var lastMerkleTreeSection = item.MerkleTreeSections[^1];

                var result = await this.ReadBlockFromFileAsync(item.FilePath, lastMerkleTreeSection, blockHash, item.MaxBlockLength, cancellationToken);
                if (result is not null) return result;
            }

            var key = GenKey(rootHash, blockHash);
            return await _blockStorage.TryReadAsync(key, cancellationToken);
        }
    }

    private async ValueTask<IMemoryOwner<byte>?> ReadBlockFromFileAsync(string filePath, MerkleTreeSection merkleTreeSection, OmniHash blockHash, int maxBlockLength, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath)) return null;
        if (!merkleTreeSection.TryGetIndex(blockHash, out var index)) return null;

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);

        var position = maxBlockLength * index;
        fileStream.Seek(position, SeekOrigin.Begin);

        var blockLength = (int)Math.Min(maxBlockLength, fileStream.Length - position);
        var memoryOwner = _bytesPool.Memory.Rent(blockLength).Shrink(blockLength);

        for (int start = 0; start < blockLength;)
        {
            start += await fileStream.ReadAsync(memoryOwner.Memory[start..], cancellationToken);
        }

        return memoryOwner;
    }

    private static string GenKey(string prefix, OmniHash blockHash)
    {
        return prefix + "/" + StringConverter.HashToString(blockHash);
    }

    private static string GenKey(OmniHash rootHash, OmniHash blockHash)
    {
        return StringConverter.HashToString(rootHash) + "/" + StringConverter.HashToString(blockHash);
    }
}
