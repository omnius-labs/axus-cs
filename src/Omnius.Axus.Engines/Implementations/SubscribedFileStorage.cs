using System.Buffers;
using System.Collections.Immutable;
using System.IO.Pipelines;
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

public sealed partial class SubscribedFileStorage : AsyncDisposableBase, ISubscribedFileStorage
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IKeyValueStorageFactory _keyValueStorageFactory;
    private readonly IBytesPool _bytesPool;
    private readonly SubscribedFileStorageOptions _options;

    private readonly SubscribedFileStorageRepository _subscriberRepo;
    private readonly IKeyValueStorage<string> _blockStorage;

    private Task _computeLoopTask = null!;

    private readonly AsyncLock _asyncLock = new();

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public static async ValueTask<SubscribedFileStorage> CreateAsync(IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, SubscribedFileStorageOptions options, CancellationToken cancellationToken = default)
    {
        var subscribedFileStorage = new SubscribedFileStorage(keyValueStorageFactory, bytesPool, options);
        await subscribedFileStorage.InitAsync(cancellationToken);
        return subscribedFileStorage;
    }

    private SubscribedFileStorage(IKeyValueStorageFactory keyValueStorageFactory, IBytesPool bytesPool, SubscribedFileStorageOptions options)
    {
        _keyValueStorageFactory = keyValueStorageFactory;
        _bytesPool = bytesPool;
        _options = options;

        _subscriberRepo = new SubscribedFileStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));
        _blockStorage = _keyValueStorageFactory.Create<string>(Path.Combine(_options.ConfigDirectoryPath, "blocks"), _bytesPool);
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

        _subscriberRepo.Dispose();
        _blockStorage.Dispose();
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
        foreach (var fileItem in _subscriberRepo.FileItems.FindAll())
        {
            if (fileItem.Status.State != SubscribedFileState.Downloading) continue;
            if (fileItem.Status.DownloadedBlockCount < fileItem.Status.TotalBlockCount) continue;

            // 最後のMerkleTreeSectionまで展開済みの場合
            if (fileItem.Status.CurrentDepth == 0)
            {
                var newFileItem = fileItem with
                {
                    Status = fileItem.Status with
                    {
                        State = SubscribedFileState.Downloaded
                    }
                };
                _subscriberRepo.FileItems.Upsert(newFileItem);
            }
            // 最後のMerkleTreeSectionまで未展開の場合
            else
            {
                var blockHashes = _subscriberRepo.BlockItems.FindBlockHashes(fileItem.RootHash, fileItem.Status.CurrentDepth);
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
                    }
                };
                var newBlockItems = nextMerkleTreeSection.Hashes
                    .Select((blockHash, order) =>
                        new SubscribedBlockItem()
                        {
                            RootHash = fileItem.RootHash,
                            BlockHash = blockHash,
                            Depth = nextMerkleTreeSection.Depth,
                            Order = order,
                        })
                    .ToArray();

                using (await _asyncLock.LockAsync(cancellationToken))
                {
                    _subscriberRepo.FileItems.Upsert(newFileItem);
                    _subscriberRepo.BlockItems.UpsertBulk(newBlockItems);
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

            foreach (var fileItem in _subscriberRepo.FileItems.FindAll())
            {
                var status = new SubscribedFileStatus(fileItem.Status.CurrentDepth, (uint)fileItem.Status.DownloadedBlockCount, (uint)fileItem.Status.TotalBlockCount, fileItem.Status.State);
                var report = new SubscribedFileReport(fileItem.RootHash, fileItem.Authors.Select(n => new Utf8String(n)).ToArray(), status);
                results.Add(report);
            }

            return results.ToArray();
        }
    }

    // TODO 実装する
    public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
    {
    }

    public async ValueTask<IEnumerable<OmniHash>> GetWantRootHashesAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var results = new List<OmniHash>();

            foreach (var fileItem in _subscriberRepo.FileItems.FindAll())
            {
                results.Add(fileItem.RootHash);
            }

            return results;
        }
    }

    public async ValueTask<IEnumerable<OmniHash>> GetWantBlockHashesAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var results = new List<OmniHash>();

            foreach (var blockItem in _subscriberRepo.BlockItems.Find(rootHash))
            {
                if (blockItem.IsDownloaded) continue;
                results.Add(blockItem.BlockHash);
            }

            return results;
        }
    }

    public async ValueTask<bool> ContainsWantContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            return _subscriberRepo.FileItems.Exists(rootHash);
        }
    }

    public async ValueTask<bool> ContainsWantBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            return _subscriberRepo.BlockItems.Exists(rootHash, blockHash);
        }
    }

    public async ValueTask SubscribeFileAsync(OmniHash rootHash, string author, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var fileItem = _subscriberRepo.FileItems.FindOne(rootHash);

            if (fileItem is not null)
            {
                if (fileItem.Authors.Contains(author)) return;

                var authors = fileItem.Authors.Append(author).ToArray();
                fileItem = fileItem with { Authors = authors };
                _subscriberRepo.FileItems.Upsert(fileItem);

                return;
            }

            var newFileItem = new SubscribedFileItem()
            {
                RootHash = rootHash,
                Authors = new[] { author },
                Status = new SubscribedFileItemStatus()
                {
                    CurrentDepth = -1,
                    TotalBlockCount = 1,
                    DownloadedBlockCount = 0,
                    State = SubscribedFileState.Downloading,
                },
            };
            var newBlockItem = new SubscribedBlockItem()
            {
                RootHash = rootHash,
                BlockHash = rootHash,
                Depth = -1,
            };

            _subscriberRepo.FileItems.Upsert(newFileItem);
            _subscriberRepo.BlockItems.Upsert(newBlockItem);
        }
    }

    public async ValueTask UnsubscribeFileAsync(OmniHash rootHash, string author, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var fileItem = _subscriberRepo.FileItems.FindOne(rootHash);
            if (fileItem is null) return;
            if (!fileItem.Authors.Contains(author)) return;

            if (fileItem.Authors.Count > 1)
            {
                var authors = fileItem.Authors.Where(n => n != author).ToArray();
                var newFileItem = fileItem with { Authors = authors };
                _subscriberRepo.FileItems.Upsert(newFileItem);

                return;
            }

            _subscriberRepo.FileItems.Delete(rootHash);

            foreach (var blockItem in _subscriberRepo.BlockItems.Find(rootHash))
            {
                if (!blockItem.IsDownloaded) continue;
                var key = GenKey(rootHash, blockItem.BlockHash);
                await _blockStorage.TryDeleteAsync(key, cancellationToken);
            }

            _subscriberRepo.BlockItems.Delete(rootHash);
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
            var fileItem = _subscriberRepo.FileItems.FindOne(rootHash);
            if (fileItem is null || fileItem.Status.State != SubscribedFileState.Downloaded) return false;

            var blockHashes = _subscriberRepo.BlockItems.FindBlockHashes(rootHash, 0);
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
                    await _blockStorage.TryDeleteAsync(key, cancellationToken);
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
            var blockItems = _subscriberRepo.BlockItems.Find(rootHash, blockHash);
            if (blockItems.Count() == 0) return;

            var key = GenKey(rootHash, blockHash);
            await _blockStorage.WriteAsync(key, memory, cancellationToken);

            var newBlockItems = blockItems.Select(n => n with { IsDownloaded = true }).ToArray();
            _subscriberRepo.BlockItems.UpsertBulk(newBlockItems);

            var fileItem = _subscriberRepo.FileItems.FindOne(rootHash);
            if (fileItem is null) return;

            var newFileItem = fileItem with
            {
                Status = fileItem.Status with
                {
                    DownloadedBlockCount = fileItem.Status.DownloadedBlockCount + 1
                }
            };
            _subscriberRepo.FileItems.Upsert(newFileItem);
        }
    }

    private static string GenKey(OmniHash rootHash, OmniHash blockHash)
    {
        return StringConverter.ToString(rootHash) + "/" + StringConverter.ToString(blockHash);
    }
}
