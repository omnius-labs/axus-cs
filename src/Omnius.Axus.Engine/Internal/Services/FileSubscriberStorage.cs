using System.Buffers;
using System.IO.Pipelines;
using Omnius.Axus.Engine.Internal.Models;
using Omnius.Axus.Engine.Internal.Repositories;
using Omnius.Axus.Engine.Internal.Repositories.Models;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Pipelines;
using Omnius.Core.Serialization;
using Omnius.Core.Storages;

namespace Omnius.Axus.Engine.Internal.Services;

internal sealed partial class FileSubscriberStorage : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IKeyValueStorageFactory _keyValueStorageFactory;
    private readonly ISystemClock _systemClock;
    private readonly IRandomBytesProvider _randomBytesProvider;
    private readonly IBytesPool _bytesPool;
    private readonly FileSubscriberStorageOptions _options;

    private readonly FileSubscriberStorageRepository _subscriberRepo;
    private readonly IKeyValueStorage _blockStorage;

    private Task _computeLoopTask = null!;

    private readonly AsyncLock _asyncLock = new();

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public static async ValueTask<FileSubscriberStorage> CreateAsync(IKeyValueStorageFactory keyValueStorageFactory,
        ISystemClock systemClock, IRandomBytesProvider randomBytesProvider, IBytesPool bytesPool, FileSubscriberStorageOptions options, CancellationToken cancellationToken = default)
    {
        var subscribedFileStorage = new FileSubscriberStorage(keyValueStorageFactory, systemClock, randomBytesProvider, bytesPool, options);
        await subscribedFileStorage.InitAsync(cancellationToken);
        return subscribedFileStorage;
    }

    private FileSubscriberStorage(IKeyValueStorageFactory keyValueStorageFactory,
        ISystemClock systemClock, IRandomBytesProvider randomBytesProvider, IBytesPool bytesPool, FileSubscriberStorageOptions options)
    {
        _keyValueStorageFactory = keyValueStorageFactory;
        _systemClock = systemClock;
        _randomBytesProvider = randomBytesProvider;
        _bytesPool = bytesPool;
        _options = options;

        _subscriberRepo = new FileSubscriberStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "status"), _bytesPool);
        _blockStorage = _keyValueStorageFactory.Create(Path.Combine(_options.ConfigDirectoryPath, "blocks"), _bytesPool);
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        await _subscriberRepo.MigrateAsync(cancellationToken);
        await _blockStorage.MigrateAsync(cancellationToken);

        _computeLoopTask = this.ComputeLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await _computeLoopTask;
        _cancellationTokenSource.Dispose();

        await _subscriberRepo.DisposeAsync();
        await _blockStorage.DisposeAsync();
    }

    private async Task ComputeLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);

            for (; ; )
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

                await this.UpdateAsync(_cancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e, "Operation Canceled");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }
    }

    private async ValueTask UpdateAsync(CancellationToken cancellationToken = default)
    {
        foreach (var fileItem in await _subscriberRepo.FileItems.GetItemsAsync(SubscribedFileState.Decoding, cancellationToken).ToListAsync())
        {
            var now = _systemClock.GetUtcNow();

            // 最後のMerkleTreeSectionまで展開済みの場合
            if (fileItem.Status.CurrentDepth == 0)
            {
                var newFileItem = fileItem with
                {
                    Status = fileItem.Status with
                    {
                        State = SubscribedFileState.Completed
                    },
                    UpdatedTime = now,
                };
                await _subscriberRepo.FileItems.UpsertAsync(newFileItem, cancellationToken);
            }
            // 最後のMerkleTreeSectionまで未展開の場合
            else
            {
                var blockHashes = await _subscriberRepo.BlockItems.GetBlockHashesAsync(fileItem.RootHash, fileItem.Status.CurrentDepth, cancellationToken).ToListAsync();
                if (blockHashes is null) continue;

                var nextMerkleTreeSection = await this.DecodeMerkleTreeSectionAsync(fileItem.RootHash, blockHashes, cancellationToken);
                if (nextMerkleTreeSection is null) continue;

                var newFileItem = fileItem with
                {
                    Status = fileItem.Status with
                    {
                        CurrentDepth = nextMerkleTreeSection.Depth,
                        TotalBlockCount = nextMerkleTreeSection.Hashes.Count,
                        DownloadedBlockCount = 0,
                        State = SubscribedFileState.Downloading,
                    },
                    UpdatedTime = now,
                };
                var newBlockItems = nextMerkleTreeSection.Hashes
                    .Select((blockHash, index) =>
                        new BlockSubscribedItem
                        {
                            RootHash = fileItem.RootHash,
                            BlockHash = blockHash,
                            Depth = nextMerkleTreeSection.Depth,
                            Index = index,
                            IsDownloaded = false,
                        })
                    .ToArray();

                using (await _asyncLock.LockAsync(cancellationToken))
                {
                    await _subscriberRepo.FileItems.UpsertAsync(newFileItem, cancellationToken);
                    await _subscriberRepo.BlockItems.UpsertBulkAsync(newBlockItems, cancellationToken);
                }
            }
        }
    }

    private async ValueTask<MerkleTreeSection?> DecodeMerkleTreeSectionAsync(OmniHash rootHash, IEnumerable<OmniHash> blockHashes, CancellationToken cancellationToken = default)
    {
        using var bytesPipe = new BytesPipe();

        foreach (var blockHash in blockHashes)
        {
            var key = GenKey(rootHash, blockHash);
            using var memoryOwner = await _blockStorage.TryReadAsync(key, cancellationToken);
            if (memoryOwner is null) return null;

            bytesPipe.Writer.Write(memoryOwner.Memory.Span);
        }

        return MerkleTreeSection.Import(bytesPipe.Reader.GetSequence(), _bytesPool);
    }

    public async ValueTask<IEnumerable<SubscribedFileReport>> GetSubscribedFileReportsAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var results = new List<SubscribedFileReport>();

            await foreach (var item in _subscriberRepo.FileItems.GetItemsAsync(cancellationToken))
            {
                var status = new SubscribedFileStatus(item.Status.CurrentDepth, (uint)item.Status.DownloadedBlockCount, (uint)item.Status.TotalBlockCount, item.Status.State);
                var report = new SubscribedFileReport(item.RootHash, status, item.Property);
                results.Add(report);
            }

            return results.ToArray();
        }
    }

    // TODO 実装する
    // public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
    // {
    // }

    public async ValueTask<IEnumerable<OmniHash>> GetWantRootHashesAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            return await _subscriberRepo.FileItems.GetRootHashesAsync(cancellationToken).ToListAsync();
        }
    }

    public async ValueTask<IEnumerable<OmniHash>> GetWantBlockHashesAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            return await _subscriberRepo.BlockItems.GetBlockHashesAsync(rootHash, false, cancellationToken).ToListAsync();
        }
    }

    public async ValueTask<bool> ContainsWantContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            return await _subscriberRepo.FileItems.ExistsAsync(rootHash, cancellationToken);
        }
    }

    public async ValueTask<bool> ContainsWantBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            return await _subscriberRepo.BlockItems.ExistsAsync(rootHash, blockHash, cancellationToken);
        }
    }

    public async ValueTask SubscribeFileAsync(OmniHash rootHash, AttachedProperty? property, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var now = _systemClock.GetUtcNow();

            var fileItem = await _subscriberRepo.FileItems.GetItemAsync(rootHash, cancellationToken);
            if (fileItem is not null)
            {
                fileItem = fileItem with { Property = property, UpdatedTime = now };
                await _subscriberRepo.FileItems.UpsertAsync(fileItem, cancellationToken);
                return;
            }

            var newFileItem = new FileSubscribedItem()
            {
                RootHash = rootHash,
                Property = property,
                Status = new FileSubscribedItemStatus()
                {
                    CurrentDepth = -1,
                    TotalBlockCount = 1,
                    DownloadedBlockCount = 0,
                    State = SubscribedFileState.Downloading,
                },
                CreatedTime = now,
                UpdatedTime = now,
            };
            var newBlockItem = new BlockSubscribedItem
            {
                RootHash = rootHash,
                BlockHash = rootHash,
                Depth = -1,
                Index = 0,
                IsDownloaded = false,
            };

            await _subscriberRepo.FileItems.UpsertAsync(newFileItem, cancellationToken);
            await _subscriberRepo.BlockItems.UpsertBulkAsync([newBlockItem], cancellationToken);
        }
    }

    public async ValueTask UnsubscribeFileAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var now = _systemClock.GetUtcNow();

            var fileItem = await _subscriberRepo.FileItems.GetItemAsync(rootHash, cancellationToken);
            if (fileItem is null) return;

            await _subscriberRepo.FileItems.DeleteAsync(rootHash, cancellationToken);

            await foreach (var blockHash in _subscriberRepo.BlockItems.GetBlockHashesAsync(rootHash, true, cancellationToken))
            {
                var key = GenKey(rootHash, blockHash);
                _ = await _blockStorage.TryDeleteAsync(key, cancellationToken);
            }

            await _subscriberRepo.BlockItems.DeleteAsync(rootHash, cancellationToken);
        }
    }

    public async ValueTask<bool> TryExportFileAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default)
    {
        bool result = false;

        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            var writer = PipeWriter.Create(fileStream);
            result = await this.TryExportFileAsync(rootHash, writer, cancellationToken);
            await writer.CompleteAsync();
        }

        return result;
    }

    public async ValueTask<bool> TryExportFileAsync(OmniHash rootHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var fileItem = await _subscriberRepo.FileItems.GetItemAsync(rootHash, cancellationToken);
            if (fileItem is null || fileItem.Status.State != SubscribedFileState.Completed) return false;

            var blockHashes = await _subscriberRepo.BlockItems.GetBlockHashesAsync(rootHash, 0).ToListAsync(cancellationToken);
            if (blockHashes is null) return false;

            foreach (var blockHash in blockHashes)
            {
                var key = GenKey(rootHash, blockHash);
                var exists = await _blockStorage.ContainsKeyAsync(key, cancellationToken);
                if (!exists) return false;
            }

            foreach (var blockHash in blockHashes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var key = GenKey(rootHash, blockHash);
                using var memoryOwner = await _blockStorage.TryReadAsync(key, cancellationToken);
                if (memoryOwner is null) return false;

                if (!blockHash.Validate(memoryOwner.Memory.Span))
                {
                    _ = await _blockStorage.TryDeleteAsync(key, cancellationToken);
                    return false;
                }

                bufferWriter.Write(memoryOwner.Memory.Span);
            }

            return true;
        }
    }

    public async ValueTask<IMemoryOwner<byte>?> TryReadBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var key = GenKey(rootHash, blockHash);
            return await _blockStorage.TryReadAsync(key, cancellationToken);
        }
    }

    public async ValueTask WriteBlockAsync(OmniHash rootHash, OmniHash blockHash, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var blockItems = await _subscriberRepo.BlockItems.GetItemsAsync(rootHash, blockHash, cancellationToken).ToListAsync(cancellationToken);
            if (blockItems.Count() == 0) return;

            var key = GenKey(rootHash, blockHash);
            await _blockStorage.WriteAsync(key, memory, cancellationToken);

            var newBlockItems = blockItems.Select(n => n with { IsDownloaded = true }).ToArray();
            await _subscriberRepo.BlockItems.UpsertBulkAsync(newBlockItems, cancellationToken);

            var fileItem = await _subscriberRepo.FileItems.GetItemAsync(rootHash, cancellationToken);
            if (fileItem is null) return;

            var newFileItem = fileItem with
            {
                Status = fileItem.Status with
                {
                    DownloadedBlockCount = fileItem.Status.DownloadedBlockCount + 1,
                    State = (fileItem.Status.TotalBlockCount == fileItem.Status.DownloadedBlockCount + 1) ? SubscribedFileState.Decoding : SubscribedFileState.Downloading,
                }
            };
            await _subscriberRepo.FileItems.UpsertAsync(newFileItem, cancellationToken);
        }
    }

    private static string GenKey(OmniHash rootHash, OmniHash blockHash) => rootHash.ToString(ConvertBaseType.Base16Lower) + "/" + blockHash.ToString(ConvertBaseType.Base16Lower);
}
