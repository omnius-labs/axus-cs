using System.Buffers;
using Omnius.Axus.Engines.Internal;
using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Engines.Internal.Repositories;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Cryptography.Functions;
using Omnius.Core.Pipelines;
using Omnius.Core.Storages;

namespace Omnius.Axus.Engines;

public sealed partial class FilePublisherStorage : AsyncDisposableBase, IFilePublisherStorage
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IKeyValueStorageFactory _keyValueStorageFactory;
    private readonly IBytesPool _bytesPool;
    private readonly FilePublisherStorageOptions _options;

    private readonly FilePublisherStorageRepository _publisherRepo;
    private readonly IKeyValueStorage<string> _blockStorage;

    private readonly AsyncLock _asyncLock = new();
    private readonly AsyncLock _publishAsyncLock = new();

    public static async ValueTask<FilePublisherStorage> CreateAsync(IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, FilePublisherStorageOptions options, CancellationToken cancellationToken = default)
    {
        var publishedFileStorage = new FilePublisherStorage(keyValueStorageFactory, bytesPool, options);
        await publishedFileStorage.InitAsync(cancellationToken);
        return publishedFileStorage;
    }

    private FilePublisherStorage(IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, FilePublisherStorageOptions options)
    {
        _keyValueStorageFactory = keyValueStorageFactory;
        _bytesPool = bytesPool;
        _options = options;

        _publisherRepo = new FilePublisherStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
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

            foreach (var item in _publisherRepo.FileItems.FindAll())
            {
                fileReports.Add(new PublishedFileReport(item.FilePath, item.RootHash));
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

            foreach (var item in _publisherRepo.FileItems.FindAll())
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
            var results = new List<OmniHash>();

            foreach (var blockItem in _publisherRepo.BlockInternalItems.Find(rootHash))
            {
                results.Add(blockItem.BlockHash);
            }

            foreach (var blockItem in _publisherRepo.BlockExternalItems.Find(rootHash))
            {
                results.Add(blockItem.BlockHash);
            }

            return results;
        }
    }

    public async ValueTask<bool> ContainsPushContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            return _publisherRepo.FileItems.Exists(rootHash);
        }
    }

    public async ValueTask<bool> ContainsPushBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            return _publisherRepo.BlockInternalItems.Exists(rootHash, blockHash)
                || _publisherRepo.BlockExternalItems.Exists(rootHash, blockHash);
        }
    }

    public async ValueTask<OmniHash> PublishFileAsync(string filePath, int maxBlockSize, string zone, CancellationToken cancellationToken = default)
    {
        using (await _publishAsyncLock.LockAsync(cancellationToken))
        {
            var fileItem = _publisherRepo.FileItems.FindOne(filePath);

            if (fileItem is not null)
            {
                if (fileItem.Authors.Contains(zone)) return fileItem.RootHash;

                var authors = fileItem.Authors.Append(zone).ToArray();
                fileItem = fileItem with { Authors = authors };
                _publisherRepo.FileItems.Upsert(fileItem);

                return fileItem.RootHash;
            }

            var tempPrefix = "_temp_" + Guid.NewGuid().ToString("N");

            var externalBlockItems = await this.ImportFromFileAsync(filePath, maxBlockSize, cancellationToken);
            var currentBlockHashes = externalBlockItems.Select(n => n.BlockHash).ToArray();

            var allInternalBlockItems = new List<BlockPublishedInternalItem>();

            for (int depth = 0; ; depth++)
            {
                using var bytesPool = new BytesPipe(_bytesPool);
                var merkleTreeSection = new MerkleTreeSection(depth, currentBlockHashes);
                merkleTreeSection.Export(bytesPool.Writer, _bytesPool);

                var localBlockItems = await this.ImportFromMemoryAsync(tempPrefix, bytesPool.Reader.GetSequence(), maxBlockSize, depth, cancellationToken);
                allInternalBlockItems.AddRange(localBlockItems);
                currentBlockHashes = localBlockItems.Select(n => n.BlockHash).ToArray();

                if (currentBlockHashes.Length == 1) break;
            }

            var rootHash = currentBlockHashes.Single();
            allInternalBlockItems = allInternalBlockItems.Select(n => n with { RootHash = rootHash }).ToList();
            externalBlockItems = externalBlockItems.Select(n => n with { RootHash = rootHash }).ToArray();

            var newFileItem = new FilePublishedItem
            {
                RootHash = rootHash,
                FilePath = filePath,
                Authors = new[] { zone },
                MaxBlockSize = maxBlockSize,
            };

            using (await _asyncLock.LockAsync(cancellationToken))
            {
                var newPrefix = StringConverter.ToString(rootHash);
                var targetBlockHashes = allInternalBlockItems.Select(n => n.BlockHash).ToArray();
                await this.RenameBlocksAsync(tempPrefix, newPrefix, targetBlockHashes, cancellationToken);

                _publisherRepo.FileItems.Upsert(newFileItem);
                _publisherRepo.BlockInternalItems.UpsertBulk(allInternalBlockItems);
                _publisherRepo.BlockExternalItems.UpsertBulk(externalBlockItems);
            }

            return rootHash;
        }
    }

    public async ValueTask<OmniHash> PublishFileAsync(ReadOnlySequence<byte> sequence, int maxBlockSize, string zone, CancellationToken cancellationToken = default)
    {
        using (await _publishAsyncLock.LockAsync(cancellationToken))
        {
            var tempPrefix = "_temp_" + Guid.NewGuid().ToString("N");

            var allInternalBlockItems = new List<BlockPublishedInternalItem>();

            var internalBlockItems = await this.ImportFromMemoryAsync(tempPrefix, sequence, maxBlockSize, 0, cancellationToken);
            allInternalBlockItems.AddRange(internalBlockItems);
            var currentBlockHashes = internalBlockItems.Select(n => n.BlockHash).ToArray();

            for (int depth = 0; ; depth++)
            {
                using var bytesPool = new BytesPipe(_bytesPool);
                var merkleTreeSection = new MerkleTreeSection(depth, currentBlockHashes);
                merkleTreeSection.Export(bytesPool.Writer, _bytesPool);

                internalBlockItems = await this.ImportFromMemoryAsync(tempPrefix, bytesPool.Reader.GetSequence(), maxBlockSize, depth, cancellationToken);
                allInternalBlockItems.AddRange(internalBlockItems);
                currentBlockHashes = internalBlockItems.Select(n => n.BlockHash).ToArray();

                if (currentBlockHashes.Length == 1) break;
            }

            var rootHash = currentBlockHashes.Single();
            allInternalBlockItems = allInternalBlockItems.Select(n => n with { RootHash = rootHash }).ToList();

            var fileItem = _publisherRepo.FileItems.FindOne(rootHash);

            if (fileItem is not null)
            {
                var targetBlockHashes = allInternalBlockItems.Select(n => n.BlockHash).ToArray();
                await this.DeleteBlocksAsync(tempPrefix, targetBlockHashes, cancellationToken);

                if (fileItem.Authors.Contains(zone)) return rootHash;

                var authors = fileItem.Authors.Append(zone).ToArray();
                fileItem = fileItem with { Authors = authors };
                _publisherRepo.FileItems.Upsert(fileItem);

                return rootHash;
            }

            var newFileItem = new FilePublishedItem
            {
                RootHash = rootHash,
                FilePath = null,
                Authors = new[] { zone },
                MaxBlockSize = maxBlockSize,
            };

            using (await _asyncLock.LockAsync(cancellationToken))
            {
                var newPrefix = StringConverter.ToString(rootHash);
                var targetBlockHashes = allInternalBlockItems.Select(n => n.BlockHash).ToArray();
                await this.RenameBlocksAsync(tempPrefix, newPrefix, targetBlockHashes, cancellationToken);

                _publisherRepo.FileItems.Upsert(newFileItem);
                _publisherRepo.BlockInternalItems.UpsertBulk(allInternalBlockItems);
            }

            return rootHash;
        }
    }

    private async ValueTask DeleteBlocksAsync(string prefix, IEnumerable<OmniHash> blockHashes, CancellationToken cancellationToken = default)
    {
        foreach (var blockHash in blockHashes.ToHashSet())
        {
            var key = GenKey(prefix, blockHash);
            await _blockStorage.TryDeleteAsync(key, cancellationToken);
        }
    }

    private async ValueTask RenameBlocksAsync(string oldPrefix, string newPrefix, IEnumerable<OmniHash> blockHashes, CancellationToken cancellationToken = default)
    {
        foreach (var blockHash in blockHashes.ToHashSet())
        {
            var oldKey = GenKey(oldPrefix, blockHash);
            var newKey = GenKey(newPrefix, blockHash);
            await _blockStorage.TryChangeKeyAsync(oldKey, newKey, cancellationToken);
        }
    }

    private async ValueTask<IEnumerable<BlockPublishedInternalItem>> ImportFromMemoryAsync(string prefix, ReadOnlySequence<byte> sequence, int maxBlockSize, int depth, CancellationToken cancellationToken = default)
    {
        var blockItems = new List<BlockPublishedInternalItem>();

        using var memoryOwner = _bytesPool.Memory.Rent(maxBlockSize).Shrink(maxBlockSize);
        int order = 0;

        while (sequence.Length > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var blockSize = (int)Math.Min(sequence.Length, maxBlockSize);
            var memory = memoryOwner.Memory[..blockSize];
            sequence.CopyTo(memory.Span);

            var blockHash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memory.Span));

            var blockItem = new BlockPublishedInternalItem
            {
                RootHash = OmniHash.Empty,
                BlockHash = blockHash,
                Depth = depth,
                Order = order++,
            };
            blockItems.Add(blockItem);

            await this.WriteBlockAsync(prefix, blockHash, memory);

            sequence = sequence.Slice(blockSize);
        }

        return blockItems;
    }

    private async ValueTask<IEnumerable<BlockPublishedExternalItem>> ImportFromFileAsync(string filePath, int maxBlockSize, CancellationToken cancellationToken = default)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        var blockItems = new List<BlockPublishedExternalItem>();

        using var memoryOwner = _bytesPool.Memory.Rent(maxBlockSize).Shrink(maxBlockSize);
        int order = 0;

        while (stream.Position < stream.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var blockSize = (int)Math.Min(maxBlockSize, stream.Length - stream.Position);
            var memory = memoryOwner.Memory[..blockSize];

            for (int position = 0; position < blockSize;)
            {
                var readSize = await stream.ReadAsync(memory[position..], cancellationToken);
                if (readSize == 0) throw new EndOfStreamException();
                position += readSize;
            }

            var blockHash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memory.Span));

            var blockItem = new BlockPublishedExternalItem
            {
                FilePath = filePath,
                RootHash = OmniHash.Empty,
                BlockHash = blockHash,
                Order = order++,
                Offset = stream.Position - blockSize,
                Count = blockSize,
            };
            blockItems.Add(blockItem);
        }

        return blockItems;
    }

    private async ValueTask WriteBlockAsync(string prefix, OmniHash blockHash, ReadOnlyMemory<byte> memory)
    {
        var key = GenKey(prefix, blockHash);
        await _blockStorage.WriteAsync(key, memory);
    }

    public async ValueTask UnpublishFileAsync(string filePath, string zone, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var fileItem = _publisherRepo.FileItems.FindOne(filePath);
            if (fileItem is null) return;

            await this.UnpublishFileAsync(fileItem.RootHash, zone, cancellationToken);
        }
    }

    public async ValueTask UnpublishFileAsync(OmniHash rootHash, string zone, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var fileItem = _publisherRepo.FileItems.FindOne(rootHash);
            if (fileItem is null) return;
            if (!fileItem.Authors.Contains(zone)) return;

            if (fileItem.Authors.Count > 1)
            {
                var authors = fileItem.Authors.Where(n => n != zone).ToArray();
                var newFileItem = fileItem with { Authors = authors };
                _publisherRepo.FileItems.Upsert(newFileItem);

                return;
            }

            _publisherRepo.FileItems.Delete(rootHash);

            foreach (var blockItem in _publisherRepo.BlockInternalItems.Find(rootHash))
            {
                var key = GenKey(rootHash, blockItem.BlockHash);
                await _blockStorage.TryDeleteAsync(key, cancellationToken);
            }

            _publisherRepo.BlockInternalItems.Delete(rootHash);
            _publisherRepo.BlockExternalItems.Delete(rootHash);
        }
    }

    public async ValueTask<IMemoryOwner<byte>?> TryReadBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var key = GenKey(rootHash, blockHash);
            var result = await _blockStorage.TryReadAsync(key, cancellationToken);
            if (result is not null) return result;

            var blockItem = _publisherRepo.BlockExternalItems.Find(rootHash, blockHash).FirstOrDefault();
            if (blockItem is null) return null;

            result = await this.TryReadBlockFromFileAsync(blockItem.FilePath, blockItem.Offset, blockItem.Count, blockHash, cancellationToken);
            return result;
        }
    }

    private async ValueTask<IMemoryOwner<byte>?> TryReadBlockFromFileAsync(string filePath, long offset, int count, OmniHash blockHash, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath)) return null;

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        fileStream.Seek(offset, SeekOrigin.Begin);
        var memoryOwner = _bytesPool.Memory.Rent(count).Shrink(count);

        for (int position = 0; position < count;)
        {
            var readSize = await fileStream.ReadAsync(memoryOwner.Memory[position..], cancellationToken);
            if (readSize == 0) throw new EndOfStreamException();
            position += readSize;
        }

        if (blockHash.AlgorithmType == OmniHashAlgorithmType.Sha2_256)
        {
            var hash = Sha2_256.ComputeHash(memoryOwner.Memory.Span);
            if (!BytesOperations.Equals(hash, blockHash.Value.Span)) return null;
        }

        return memoryOwner;
    }

    private static string GenKey(string prefix, OmniHash blockHash)
    {
        return prefix + "/" + StringConverter.ToString(blockHash);
    }

    private static string GenKey(OmniHash rootHash, OmniHash blockHash)
    {
        return StringConverter.ToString(rootHash) + "/" + StringConverter.ToString(blockHash);
    }

    public ValueTask<IEnumerable<PublishedFileReport>> GetPublishedFileReportsAsync(string zone, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
