using System.Buffers;
using Omnius.Axus.Engines.Implementations.Internal.Repositories;
using Omnius.Axus.Engines.Internal;
using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Cryptography.Functions;
using Omnius.Core.Pipelines;
using Omnius.Core.Serialization;
using Omnius.Core.Storages;

namespace Omnius.Axus.Engines;

public sealed partial class FilePublisherStorage : AsyncDisposableBase, IFilePublisherStorage
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IKeyValueStorageFactory _keyValueStorageFactory;
    private readonly ISystemClock _systemClock;
    private readonly IRandomBytesProvider _randomBytesProvider;
    private readonly IBytesPool _bytesPool;
    private readonly FilePublisherStorageOptions _options;

    private readonly FilePublisherStorageRepository _publisherRepo;
    private readonly IKeyValueStorage _blockStorage;

    private readonly AsyncLock _asyncLock = new();
    private readonly AsyncLock _publishAsyncLock = new();

    private static Base16 _base16 = new Base16(Base16Case.Lower);

    public static async ValueTask<FilePublisherStorage> CreateAsync(IKeyValueStorageFactory keyValueStorageFactory,
        ISystemClock systemClock, IRandomBytesProvider randomBytesProvider, IBytesPool bytesPool, FilePublisherStorageOptions options, CancellationToken cancellationToken = default)
    {
        var publishedFileStorage = new FilePublisherStorage(keyValueStorageFactory, systemClock, randomBytesProvider, bytesPool, options);
        await publishedFileStorage.InitAsync(cancellationToken);
        return publishedFileStorage;
    }

    private FilePublisherStorage(IKeyValueStorageFactory keyValueStorageFactory,
        ISystemClock systemClock, IRandomBytesProvider randomBytesProvider, IBytesPool bytesPool, FilePublisherStorageOptions options)
    {
        _keyValueStorageFactory = keyValueStorageFactory;
        _systemClock = systemClock;
        _randomBytesProvider = randomBytesProvider;
        _bytesPool = bytesPool;
        _options = options;

        _publisherRepo = new FilePublisherStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "status"), _bytesPool);
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

    public async ValueTask<IEnumerable<PublishedFileReport>> GetPublishedFileReportsAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var fileReports = new List<PublishedFileReport>();

            await foreach (var item in _publisherRepo.FileItems.GetItemsAsync(cancellationToken))
            {
                fileReports.Add(new PublishedFileReport(item.FilePath, item.RootHash, item.Properties.ToArray()));
            }

            return fileReports.ToArray();
        }
    }

    // TODO 実装する
    public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
    {
        // 一時的なブロックが残留する可能性があるので、消す必要がある
    }

    public async ValueTask<IEnumerable<OmniHash>> GetPushRootHashesAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var results = new List<OmniHash>();

            await foreach (var rootHash in _publisherRepo.FileItems.GetRootHashesAsync(cancellationToken))
            {
                results.Add(rootHash);
            }

            return results;
        }
    }

    public async ValueTask<IEnumerable<OmniHash>> GetPushBlockHashesAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var results = new List<OmniHash>();

            await foreach (var blockItem in _publisherRepo.BlockInternalItems.GetItemsAsync(rootHash))
            {
                results.Add(blockItem.BlockHash);
            }

            await foreach (var blockItem in _publisherRepo.BlockExternalItems.GetItemsAsync(rootHash))
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
            return await _publisherRepo.FileItems.ExistsAsync(rootHash, cancellationToken);
        }
    }

    public async ValueTask<bool> ContainsPushBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            return await _publisherRepo.BlockInternalItems.ExistsAsync(rootHash, blockHash)
                || await _publisherRepo.BlockExternalItems.ExistsAsync(rootHash, blockHash);
        }
    }

    public async ValueTask<OmniHash> PublishFileAsync(string filePath, int maxBlockSize, IEnumerable<AttachedProperty> properties, CancellationToken cancellationToken = default)
    {
        using (await _publishAsyncLock.LockAsync(cancellationToken))
        {
            var now = _systemClock.GetUtcNow();

            var fileItem = await _publisherRepo.FileItems.GetItemAsync(filePath, cancellationToken);
            if (fileItem is not null)
            {
                fileItem = fileItem with { Properties = properties.ToArray(), UpdatedTime = now };
                await _publisherRepo.FileItems.UpsertAsync(fileItem, cancellationToken);
                return fileItem.RootHash;
            }

            string tempSpaceId = this.GenSpaceId(now);

            var externalBlockItems = await this.ImportFromFileAsync(filePath, maxBlockSize, cancellationToken);
            var currentBlockHashes = externalBlockItems.Select(n => n.BlockHash).ToArray();

            var allInternalBlockItems = new List<BlockPublishedInternalItem>();

            for (int depth = 0; ; depth++)
            {
                using var bytesPool = new BytesPipe(_bytesPool);
                var merkleTreeSection = new MerkleTreeSection(depth, currentBlockHashes);
                merkleTreeSection.Export(bytesPool.Writer, _bytesPool);

                var localBlockItems = await this.ImportFromMemoryAsync(tempSpaceId, bytesPool.Reader.GetSequence(), maxBlockSize, depth, cancellationToken);
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
                Properties = properties.ToArray(),
                MaxBlockSize = maxBlockSize,
                CreatedTime = now,
                UpdatedTime = now,
            };

            using (await _asyncLock.LockAsync(cancellationToken))
            {
                var targetBlockHashes = allInternalBlockItems.Select(n => n.BlockHash).ToArray();
                await this.RenameBlocksAsync(tempSpaceId, rootHash, targetBlockHashes, cancellationToken);

                await _publisherRepo.FileItems.UpsertAsync(newFileItem, cancellationToken);
                await _publisherRepo.BlockInternalItems.UpsertBulkAsync(allInternalBlockItems, cancellationToken);
                await _publisherRepo.BlockExternalItems.UpsertBulkAsync(externalBlockItems, cancellationToken);
            }

            return rootHash;
        }
    }

    public async ValueTask<OmniHash> PublishFileAsync(ReadOnlySequence<byte> sequence, int maxBlockSize, IEnumerable<AttachedProperty> properties, CancellationToken cancellationToken = default)
    {
        using (await _publishAsyncLock.LockAsync(cancellationToken))
        {
            var now = _systemClock.GetUtcNow();
            var tempSpaceId = this.GenSpaceId(now);

            var allInternalBlockItems = new List<BlockPublishedInternalItem>();

            var internalBlockItems = await this.ImportFromMemoryAsync(tempSpaceId, sequence, maxBlockSize, 0, cancellationToken);
            allInternalBlockItems.AddRange(internalBlockItems);
            var currentBlockHashes = internalBlockItems.Select(n => n.BlockHash).ToArray();

            for (int depth = 0; ; depth++)
            {
                using var bytesPool = new BytesPipe(_bytesPool);
                var merkleTreeSection = new MerkleTreeSection(depth, currentBlockHashes);
                merkleTreeSection.Export(bytesPool.Writer, _bytesPool);

                internalBlockItems = await this.ImportFromMemoryAsync(tempSpaceId, bytesPool.Reader.GetSequence(), maxBlockSize, depth, cancellationToken);
                allInternalBlockItems.AddRange(internalBlockItems);
                currentBlockHashes = internalBlockItems.Select(n => n.BlockHash).ToArray();

                if (currentBlockHashes.Length == 1) break;
            }

            var rootHash = currentBlockHashes.Single();
            allInternalBlockItems = allInternalBlockItems.Select(n => n with { RootHash = rootHash }).ToList();

            var fileItem = await _publisherRepo.FileItems.GetItemAsync(rootHash, cancellationToken);

            if (fileItem is not null)
            {
                fileItem = fileItem with { Properties = properties.ToArray(), UpdatedTime = now };
                await _publisherRepo.FileItems.UpsertAsync(fileItem, cancellationToken);

                var targetBlockHashes = allInternalBlockItems.Select(n => n.BlockHash).ToArray();
                await this.DeleteBlocksAsync(tempSpaceId, targetBlockHashes, cancellationToken);

                return rootHash;
            }

            var newFileItem = new FilePublishedItem
            {
                RootHash = rootHash,
                FilePath = null,
                Properties = properties.ToArray(),
                MaxBlockSize = maxBlockSize,
                CreatedTime = now,
                UpdatedTime = now,
            };

            using (await _asyncLock.LockAsync(cancellationToken))
            {
                var targetBlockHashes = allInternalBlockItems.Select(n => n.BlockHash).ToArray();
                await this.RenameBlocksAsync(tempSpaceId, rootHash, targetBlockHashes, cancellationToken);

                await _publisherRepo.FileItems.UpsertAsync(newFileItem, cancellationToken);
                await _publisherRepo.BlockInternalItems.UpsertBulkAsync(allInternalBlockItems, cancellationToken);
            }

            return rootHash;
        }
    }

    private string GenSpaceId(DateTime now)
    {
        return string.Format("{}_{}", now.ToString("yyyy-MM-dd'T'HH:mm:ss"), _base16.BytesToString(new ReadOnlySequence<byte>(_randomBytesProvider.GetBytes(32))));
    }

    private async ValueTask DeleteBlocksAsync(string prefix, IEnumerable<OmniHash> blockHashes, CancellationToken cancellationToken = default)
    {
        foreach (var blockHash in blockHashes.ToHashSet())
        {
            var key = GenTempKey(prefix, blockHash);
            await _blockStorage.TryDeleteAsync(key, cancellationToken);
        }
    }

    private async ValueTask RenameBlocksAsync(string spaceId, OmniHash rootHash, IEnumerable<OmniHash> blockHashes, CancellationToken cancellationToken = default)
    {
        foreach (var blockHash in blockHashes.ToHashSet())
        {
            var oldKey = GenTempKey(spaceId, blockHash);
            var newKey = GenFixedKey(rootHash, blockHash);
            await _blockStorage.TryChangeKeyAsync(oldKey, newKey, cancellationToken);
        }
    }

    private async ValueTask<IEnumerable<BlockPublishedInternalItem>> ImportFromMemoryAsync(string spaceId, ReadOnlySequence<byte> sequence, int maxBlockSize, int depth, CancellationToken cancellationToken = default)
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

            await this.WriteBlockAsync(spaceId, blockHash, memory);

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
                Length = blockSize,
            };
            blockItems.Add(blockItem);
        }

        return blockItems;
    }

    private async ValueTask WriteBlockAsync(string spaceId, OmniHash blockHash, ReadOnlyMemory<byte> memory)
    {
        var key = GenTempKey(spaceId, blockHash);
        await _blockStorage.WriteAsync(key, memory);
    }

    public async ValueTask UnpublishFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var fileItem = await _publisherRepo.FileItems.GetItemAsync(filePath, cancellationToken);
            if (fileItem is null) return;

            await this.UnpublishFileAsync(fileItem.RootHash, cancellationToken);
        }
    }

    public async ValueTask UnpublishFileAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var fileItem = await _publisherRepo.FileItems.GetItemAsync(rootHash, cancellationToken);
            if (fileItem is null) return;

            await _publisherRepo.FileItems.DeleteAsync(rootHash, cancellationToken);

            await foreach (var blockItem in _publisherRepo.BlockInternalItems.GetItemsAsync(rootHash, cancellationToken))
            {
                var key = GenFixedKey(rootHash, blockItem.BlockHash);
                await _blockStorage.TryDeleteAsync(key, cancellationToken);
            }

            await _publisherRepo.BlockInternalItems.DeleteAsync(rootHash, cancellationToken);
            await _publisherRepo.BlockExternalItems.DeleteAsync(rootHash, cancellationToken);
        }
    }

    public async ValueTask<IMemoryOwner<byte>?> TryReadBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var key = GenFixedKey(rootHash, blockHash);
            var result = await _blockStorage.TryReadAsync(key, cancellationToken);
            if (result is not null) return result;

            var blockItem = await _publisherRepo.BlockExternalItems.GetItemAsync(rootHash, blockHash);
            if (blockItem is null) return null;

            result = await this.TryReadBlockFromFileAsync(blockItem.FilePath, blockItem.Offset, blockItem.Length, blockHash, cancellationToken);
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

    private static string GenTempKey(string spaceId, OmniHash blockHash)
    {
        return string.Format("tmp/{}/{}", spaceId, blockHash.ToString(ConvertStringType.Base16Lower));
    }

    private static string GenFixedKey(OmniHash rootHash, OmniHash blockHash)
    {
        return string.Format("fix/{}/{}", rootHash.ToString(ConvertStringType.Base16Lower), blockHash.ToString(ConvertStringType.Base16Lower));
    }
}
