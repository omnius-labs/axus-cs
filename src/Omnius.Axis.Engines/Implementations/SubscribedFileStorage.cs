using System.Buffers;
using System.IO.Pipelines;
using Omnius.Axis.Engines.Internal;
using Omnius.Axis.Engines.Internal.Models;
using Omnius.Axis.Engines.Internal.Repositories;
using Omnius.Axis.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Pipelines;
using Omnius.Core.Storages;

namespace Omnius.Axis.Engines;

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

        _subscriberRepo = new SubscribedFileStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "state"));
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

                await this.UpdateDecodedFileItemAsync(_cancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);
        }
        catch (Exception e)
        {
            _logger.Error(e);

            throw;
        }
    }

    private async ValueTask UpdateDecodedFileItemAsync(CancellationToken cancellationToken = default)
    {
        foreach (var decodedItem in _subscriberRepo.DecodedItems.FindAll())
        {
            if (decodedItem.State == SubscribedFileState.Downloaded) continue;

            var lastMerkleTreeSection = decodedItem.MerkleTreeSections[^1];

            var completed = await this.IsDownloadCompletedAsync(decodedItem.RootHash, lastMerkleTreeSection.Hashes, cancellationToken);
            if (!completed) continue;

            if (lastMerkleTreeSection.Depth == 0) // 最後のMerkleTreeSectionまで展開済み
            {
                _subscriberRepo.DecodedItems.Upsert(new DecodedFileItem(decodedItem.RootHash, decodedItem.MerkleTreeSections.ToArray(), SubscribedFileState.Downloaded));
            }
            else // 最後のMerkleTreeSectionまで未展開
            {
                var nextMerkleTreeSection = await this.DecodeMerkleTreeSectionAsync(decodedItem.RootHash, lastMerkleTreeSection.Hashes, cancellationToken);
                if (nextMerkleTreeSection is null) continue;

                _subscriberRepo.DecodedItems.Upsert(new DecodedFileItem(decodedItem.RootHash, decodedItem.MerkleTreeSections.Append(nextMerkleTreeSection).ToArray(), SubscribedFileState.Downloading));
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

    public async ValueTask<SubscribedFileStorageReport> GetReportAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var fileReports = new List<SubscribedFileReport>();

            foreach (var decodedItem in _subscriberRepo.Items.FindAll())
            {
                var downloadingStatus = await this.ComputeDownloadingStatusAsync(decodedItem.RootHash, cancellationToken);
                var report = new SubscribedFileReport(decodedItem.RootHash, decodedItem.Registrant, downloadingStatus);
                fileReports.Add(report);
            }

            return new SubscribedFileStorageReport(fileReports.ToArray());
        }
    }

    private async ValueTask<SubscribedFileStatus> ComputeDownloadingStatusAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        var decodedItem = _subscriberRepo.DecodedItems.FindOne(rootHash);
        if (decodedItem is null) return new SubscribedFileStatus(-1, 0, 0, SubscribedFileState.Unknown);

        var lastMerkleTreeSection = decodedItem.MerkleTreeSections[^1];

        int downloadedBlockCount = await this.GetDownloadedBlockCountAsync(rootHash, lastMerkleTreeSection.Hashes, cancellationToken);
        return new SubscribedFileStatus(lastMerkleTreeSection.Depth, (uint)downloadedBlockCount, (uint)lastMerkleTreeSection.Hashes.Count(), decodedItem.State);
    }

    private async ValueTask<bool> IsDownloadCompletedAsync(OmniHash rootHash, IEnumerable<OmniHash> blockHashes, CancellationToken cancellationToken = default)
    {
        foreach (var blockHash in blockHashes)
        {
            var key = GenKey(rootHash, blockHash);
            var exists = await _blockStorage.ContainsKeyAsync(key, cancellationToken);
            if (!exists) return false;
        }

        return true;
    }

    private async ValueTask<int> GetDownloadedBlockCountAsync(OmniHash rootHash, IEnumerable<OmniHash> blockHashes, CancellationToken cancellationToken = default)
    {
        int result = 0;

        foreach (var blockHash in blockHashes)
        {
            var key = GenKey(rootHash, blockHash);
            var exists = await _blockStorage.ContainsKeyAsync(key, cancellationToken);
            if (exists) result++;
        }

        return result;
    }

    // TODO: 実装する
    public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
    {
    }

    public async ValueTask<IEnumerable<OmniHash>> GetWantContentHashesAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var results = new List<OmniHash>();

            foreach (var item in _subscriberRepo.DecodedItems.FindAll())
            {
                if (item.State == SubscribedFileState.Downloaded) continue;
                results.Add(item.RootHash);
            }

            return results;
        }
    }

    public async ValueTask<IEnumerable<OmniHash>> GetWantBlockHashesAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var decodedItem = _subscriberRepo.DecodedItems.FindOne(rootHash);
            if (decodedItem is null || decodedItem.State == SubscribedFileState.Downloaded) return Enumerable.Empty<OmniHash>();

            var lastMerkleTreeSection = decodedItem.MerkleTreeSections[^1];

            var results = new List<OmniHash>();

            foreach (var blockHash in lastMerkleTreeSection.Hashes)
            {
                var key = GenKey(rootHash, blockHash);
                if (await _blockStorage.ContainsKeyAsync(key, cancellationToken)) continue;

                results.Add(blockHash);
            }

            return results;
        }
    }

    public async ValueTask<bool> ContainsWantContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var decodedItem = _subscriberRepo.DecodedItems.FindOne(rootHash);
            if (decodedItem is null || decodedItem.State == SubscribedFileState.Downloaded) return false;

            return true;
        }
    }

    public async ValueTask<bool> ContainsWantBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var decodedItem = _subscriberRepo.DecodedItems.FindOne(rootHash);
            if (decodedItem is null || decodedItem.State == SubscribedFileState.Downloaded) return false;

            if (!decodedItem.MerkleTreeSections.Any(n => n.Contains(blockHash))) return false;

            var key = GenKey(rootHash, blockHash);
            return await _blockStorage.ContainsKeyAsync(key, cancellationToken);
        }
    }

    public async ValueTask SubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            _subscriberRepo.Items.Upsert(new SubscribedFileItem(rootHash, registrant));

            if (_subscriberRepo.DecodedItems.Exists(rootHash)) return;
            _subscriberRepo.DecodedItems.Upsert(new DecodedFileItem(rootHash, new[] { new MerkleTreeSection(-1, new[] { rootHash }) }, SubscribedFileState.Downloading));
        }
    }

    public async ValueTask UnsubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            _subscriberRepo.Items.Delete(rootHash, registrant);

            if (_subscriberRepo.Items.Exists(rootHash)) return;

            var decodedItem = _subscriberRepo.DecodedItems.FindOne(rootHash);
            if (decodedItem is null) return;

            foreach (var blockHash in decodedItem.MerkleTreeSections.SelectMany(n => n.Hashes))
            {
                var key = GenKey(rootHash, blockHash);
                await _blockStorage.TryDeleteAsync(key, cancellationToken);
            }

            _subscriberRepo.DecodedItems.Delete(rootHash);
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
            var decodedItem = _subscriberRepo.DecodedItems.FindOne(rootHash);
            if (decodedItem is null) return false;

            var lastMerkleTreeSection = decodedItem.MerkleTreeSections[^1];
            if (lastMerkleTreeSection.Depth != 0) return false; // 最後のMerkleTreeSectionまで未展開

            foreach (var blockHash in lastMerkleTreeSection.Hashes)
            {
                var key = GenKey(rootHash, blockHash);
                var exists = await _blockStorage.ContainsKeyAsync(key, cancellationToken);
                if (!exists) return false;
            }

            foreach (var blockHash in lastMerkleTreeSection.Hashes)
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

    public async ValueTask<IMemoryOwner<byte>?> ReadBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var decodedItem = _subscriberRepo.DecodedItems.FindOne(rootHash);
            if (decodedItem is null || decodedItem.State == SubscribedFileState.Downloaded) return null;

            if (!decodedItem.MerkleTreeSections.Any(n => n.Contains(blockHash))) return null;

            var key = GenKey(rootHash, blockHash);
            return await _blockStorage.TryReadAsync(key, cancellationToken);
        }
    }

    public async ValueTask WriteBlockAsync(OmniHash rootHash, OmniHash blockHash, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var decodedItem = _subscriberRepo.DecodedItems.FindOne(rootHash);
            if (decodedItem is null || decodedItem.State == SubscribedFileState.Downloaded) return;

            if (!decodedItem.MerkleTreeSections.Any(n => n.Contains(blockHash))) return;

            var key = GenKey(rootHash, blockHash);
            await _blockStorage.TryWriteAsync(key, memory, cancellationToken);
        }
    }

    private static string GenKey(OmniHash rootHash, OmniHash blockHash)
    {
        return StringConverter.HashToString(rootHash) + "/" + StringConverter.HashToString(blockHash);
    }
}
