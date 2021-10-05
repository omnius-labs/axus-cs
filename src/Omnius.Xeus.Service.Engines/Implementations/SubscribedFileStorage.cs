using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Pipelines;
using Omnius.Core.Storages;
using Omnius.Core.Streams;
using Omnius.Xeus.Service.Engines.Internal;
using Omnius.Xeus.Service.Engines.Internal.Models;
using Omnius.Xeus.Service.Engines.Internal.Repositories;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines
{
    public sealed partial class SubscribedFileStorage : AsyncDisposableBase, ISubscribedFileStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IBytesStorageFactory _bytesStorageFactory;
        private readonly IBytesPool _bytesPool;
        private readonly SubscribedFileStorageOptions _options;

        private readonly SubscribedFileStorageRepository _subscriberRepo;
        private readonly IBytesStorage<string> _blockStorage;

        private Task _computeLoopTask = null!;

        private readonly AsyncReaderWriterLock _asyncLock = new();

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public static async ValueTask<SubscribedFileStorage> CreateAsync(IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, SubscribedFileStorageOptions options, CancellationToken cancellationToken = default)
        {
            var subscribedFileStorage = new SubscribedFileStorage(bytesStorageFactory, bytesPool, options);
            await subscribedFileStorage.InitAsync(cancellationToken);
            return subscribedFileStorage;
        }

        private SubscribedFileStorage(IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, SubscribedFileStorageOptions options)
        {
            _bytesStorageFactory = bytesStorageFactory;
            _bytesPool = bytesPool;
            _options = options;

            _subscriberRepo = new SubscribedFileStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "state"));
            _blockStorage = _bytesStorageFactory.Create<string>(Path.Combine(_options.ConfigDirectoryPath, "blocks"), _bytesPool);
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
                for (; ; )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

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
            var decodedItems = _subscriberRepo.DecodedItems.FindAll();

            foreach (var decodedItem in decodedItems)
            {
                var lastMerkleTreeSection = decodedItem.MerkleTreeSections[^1];
                if (lastMerkleTreeSection.Depth == 0) continue;

                var completed = await this.ValidateDecodingCompletedAsync(decodedItem.RootHash, lastMerkleTreeSection.Hashes, cancellationToken);
                if (!completed) continue;

                var nextMerkleTreeSection = await this.DecodeMerkleTreeSectionAsync(decodedItem.RootHash, lastMerkleTreeSection.Hashes, cancellationToken);
                if (nextMerkleTreeSection is null) continue;

                lock (await _asyncLock.WriterLockAsync(cancellationToken))
                {
                    _subscriberRepo.DecodedItems.Upsert(new DecodedFileItem(decodedItem.RootHash, decodedItem.MerkleTreeSections.Append(nextMerkleTreeSection).ToArray()));
                }
            }
        }

        private async ValueTask<bool> ValidateDecodingCompletedAsync(OmniHash rootHash, IEnumerable<OmniHash> blockHashes, CancellationToken cancellationToken = default)
        {
            foreach (var blockHash in blockHashes)
            {
                var key = ComputeKey(StringConverter.HashToString(rootHash), blockHash);
                var exists = await _blockStorage.ContainsKeyAsync(key, cancellationToken);
                if (!exists) return false;
            }

            return true;
        }

        private async ValueTask<MerkleTreeSection?> DecodeMerkleTreeSectionAsync(OmniHash rootHash, IEnumerable<OmniHash> blockHashes, CancellationToken cancellationToken = default)
        {
            using var bytesPipe = new BytesPipe();

            foreach (var blockHash in blockHashes)
            {
                var blockName = ComputeKey(StringConverter.HashToString(rootHash), blockHash);
                using var memoryOwner = await _blockStorage.TryReadAsync(blockName, cancellationToken);
                if (memoryOwner is null) return null;

                bytesPipe.Writer.Write(memoryOwner.Memory.Span);
            }

            return MerkleTreeSection.Import(bytesPipe.Reader.GetSequence(), _bytesPool);
        }

        public async ValueTask<SubscribedFileStorageReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var fileReports = new List<SubscribedFileReport>();

                foreach (var item in _subscriberRepo.Items.FindAll())
                {
                    fileReports.Add(new SubscribedFileReport(item.RootHash, item.Registrant));
                }

                return new SubscribedFileStorageReport(fileReports.ToArray());
            }
        }

        public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
        }

        public async ValueTask<IEnumerable<OmniHash>> GetRootHashesAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var results = new List<OmniHash>();

                foreach (var status in _subscriberRepo.Items.FindAll())
                {
                    results.Add(status.RootHash);
                }

                return results;
            }
        }

        public async ValueTask<IEnumerable<OmniHash>> GetBlockHashesAsync(OmniHash rootHash, bool? exists = null, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var decodedItem = _subscriberRepo.DecodedItems.FindOne(rootHash);
                if (decodedItem is null) return Enumerable.Empty<OmniHash>();

                var blockHashes = decodedItem.MerkleTreeSections.SelectMany(n => n.Hashes).ToArray();

                if (exists is null)
                {
                    return blockHashes;
                }
                else
                {
                    var filteredHashes = new List<OmniHash>();

                    foreach (var blockHash in blockHashes)
                    {
                        var key = ComputeKey(StringConverter.HashToString(rootHash), blockHash);
                        if (await _blockStorage.ContainsKeyAsync(key, cancellationToken) != exists.Value) continue;

                        filteredHashes.Add(blockHash);
                    }

                    return filteredHashes;
                }
            }
        }

        public async ValueTask<bool> ContainsFileAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                if (!_subscriberRepo.Items.Exists(rootHash)) return false;

                return true;
            }
        }

        public async ValueTask<bool> ContainsBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var decodedItem = _subscriberRepo.DecodedItems.FindOne(rootHash);
                if (decodedItem is null) return false;

                return decodedItem.MerkleTreeSections.Any(n => n.Contains(blockHash));
            }
        }

        public async ValueTask SubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                _subscriberRepo.Items.Upsert(new SubscribedFileItem(rootHash, registrant));
                _subscriberRepo.DecodedItems.Upsert(new DecodedFileItem(rootHash, new[] { new MerkleTreeSection(-1, 0, 0, new[] { rootHash }) }));
            }
        }

        public async ValueTask UnsubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                _subscriberRepo.Items.Delete(rootHash, registrant);

                if (_subscriberRepo.Items.Exists(rootHash)) return;

                var decodedItem = _subscriberRepo.DecodedItems.FindOne(rootHash);
                if (decodedItem is null) return;

                foreach (var blockHash in decodedItem.MerkleTreeSections.SelectMany(n => n.Hashes))
                {
                    var key = ComputeKey(StringConverter.HashToString(rootHash), blockHash);
                    await _blockStorage.TryDeleteAsync(key, cancellationToken);
                }

                _subscriberRepo.DecodedItems.Delete(rootHash);
            }
        }

        public async ValueTask<bool> ExportFileAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default)
        {
            bool result = false;

            using (var fileStream = new UnbufferedFileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, FileOptions.None, _bytesPool))
            {
                var writer = PipeWriter.Create(fileStream);
                result = await this.ExportFileAsync(rootHash, writer, cancellationToken);
                await writer.CompleteAsync();
            }

            if (!result && File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return result;
        }

        public async ValueTask<bool> ExportFileAsync(OmniHash rootHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var decodedItem = _subscriberRepo.DecodedItems.FindOne(rootHash);
                if (decodedItem is null) return false;

                var lastMerkleTreeSection = decodedItem.MerkleTreeSections[^1];
                if (lastMerkleTreeSection.Depth != 0) return false;

                foreach (var blockHash in lastMerkleTreeSection.Hashes)
                {
                    var key = ComputeKey(StringConverter.HashToString(rootHash), blockHash);
                    var exists = await _blockStorage.ContainsKeyAsync(key, cancellationToken);
                    if (!exists) return false;
                }

                foreach (var blockHash in lastMerkleTreeSection.Hashes)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var key = ComputeKey(StringConverter.HashToString(rootHash), blockHash);
                    using var memoryOwner = await _blockStorage.TryReadAsync(key, cancellationToken);
                    if (memoryOwner is null) return false;

                    if (!blockHash.Validate(memoryOwner.Memory.Span))
                    {
                        await _blockStorage.TryDeleteAsync(key, cancellationToken);
                        return false;
                    }

                    bufferWriter.Write(memoryOwner.Memory.Span);
                    bufferWriter.Advance(memoryOwner.Memory.Length);
                }

                return true;
            }
        }

        public async ValueTask<IMemoryOwner<byte>?> ReadBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var decodedItem = _subscriberRepo.DecodedItems.FindOne(rootHash);
                if (decodedItem is null) return null;
                if (!decodedItem.MerkleTreeSections.Any(n => n.Contains(blockHash))) return null;

                var key = ComputeKey(StringConverter.HashToString(rootHash), blockHash);
                return await _blockStorage.TryReadAsync(key, cancellationToken);
            }
        }

        public async ValueTask WriteBlockAsync(OmniHash rootHash, OmniHash blockHash, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                var decodedItem = _subscriberRepo.DecodedItems.FindOne(rootHash);
                if (decodedItem is null) return;
                if (!decodedItem.MerkleTreeSections.Any(n => n.Contains(blockHash))) return;

                var key = ComputeKey(StringConverter.HashToString(rootHash), blockHash);
                await _blockStorage.WriteAsync(key, memory, cancellationToken);
            }
        }

        private static string ComputeKey(string prefix, OmniHash blockHash)
        {
            return prefix + "/" + StringConverter.HashToString(blockHash);
        }
    }
}
