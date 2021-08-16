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
using Omnius.Xeus.Engines.Internal;
using Omnius.Xeus.Engines.Internal.Models;
using Omnius.Xeus.Engines.Internal.Repositories;
using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines
{
    public record SubscribedFileStorageOptions
    {
        public string? ConfigDirectoryPath { get; init; }
        public IBytesStorageFactory? BytesStorageFactory { get; init; }
        public IBytesPool? BytesPool { get; init; }
    }

    public sealed partial class SubscribedFileStorage : AsyncDisposableBase, ISubscribedFileStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly SubscribedFileStorageOptions _options;

        private readonly SubscribedFileStorageRepository _subscriberRepo;
        private readonly IBytesStorage<string> _blockStorage;

        private Task _computeLoopTask = null!;

        private readonly AsyncReaderWriterLock _asyncLock = new();

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private SubscribedFileStorage(SubscribedFileStorageOptions options)
        {
            _options = options;

            _subscriberRepo = new SubscribedFileStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "state"));
            _blockStorage = _options.BytesStorageFactory.Create<string>(Path.Combine(_options.ConfigDirectoryPath, "blocks"), _options.BytesPool);
        }

        internal async ValueTask InitAsync(CancellationToken cancellationToken = default)
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
                    _subscriberRepo.DecodedItems.Insert(new DecodedFileItem(decodedItem.RootHash, decodedItem.MerkleTreeSections.Append(nextMerkleTreeSection).ToArray()));
                }
            }
        }

        private async ValueTask<bool> ValidateDecodingCompletedAsync(OmniHash rootHash, IEnumerable<OmniHash> blockHashes, CancellationToken cancellationToken = default)
        {
            foreach (var blockHash in blockHashes)
            {
                var blockName = ComputeBlockName(StringConverter.HashToString(rootHash), blockHash);
                var exists = await _blockStorage.ContainsKeyAsync(blockName, cancellationToken);
                if (!exists) return false;
            }

            return true;
        }

        private async ValueTask<MerkleTreeSection?> DecodeMerkleTreeSectionAsync(OmniHash rootHash, IEnumerable<OmniHash> blockHashes, CancellationToken cancellationToken = default)
        {
            using var bytesPipe = new BytesPipe();

            foreach (var blockHash in blockHashes)
            {
                var blockName = ComputeBlockName(StringConverter.HashToString(rootHash), blockHash);
                using var memoryOwner = await _blockStorage.TryReadAsync(blockName, cancellationToken);
                if (memoryOwner is null) return null;

                bytesPipe.Writer.Write(memoryOwner.Memory.Span);
            }

            return MerkleTreeSection.Import(bytesPipe.Reader.GetSequence(), _options.BytesPool);
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
                if (exists.HasValue && !exists.Value) return Enumerable.Empty<OmniHash>();

                if (!_subscriberRepo.Items.Exists(rootHash)) return Enumerable.Empty<OmniHash>();

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
                        var blockName = ComputeBlockName(StringConverter.HashToString(rootHash), blockHash);
                        if (await _blockStorage.ContainsKeyAsync(blockName, cancellationToken) != exists.Value) continue;

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

                return decodedItem.MerkleTreeSections.Any(n => n.Hashes.Contains(blockHash));
            }
        }

        public async ValueTask SubscribeFileAsync(OmniHash rootHash, string registrant, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                _subscriberRepo.Items.Insert(new SubscribedFileItem(rootHash, registrant));
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
                    var blockName = ComputeBlockName(StringConverter.HashToString(rootHash), blockHash);
                    await _blockStorage.TryDeleteAsync(blockName, cancellationToken);
                }

                _subscriberRepo.DecodedItems.Delete(rootHash);
            }
        }

        public async ValueTask<bool> ExportFileAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default)
        {
            bool result = false;

            try
            {
                using var fileStream = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None, _options.BytesPool);
                var writer = PipeWriter.Create(fileStream);
                result = await this.ExportFileAsync(rootHash, writer, cancellationToken);
                await writer.CompleteAsync();
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            return result;
        }

        public async ValueTask<bool> ExportFileAsync(OmniHash rootHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var decodedItem = _subscriberRepo.DecodedItems.FindOne(rootHash);
                if (decodedItem is null)
                {
                    return false;
                }

                var lastMerkleTreeSection = decodedItem.MerkleTreeSections[^1];
                if (lastMerkleTreeSection.Depth != 0)
                {
                    return false;
                }

                foreach (var blockHash in lastMerkleTreeSection.Hashes)
                {
                    var blockName = ComputeBlockName(StringConverter.HashToString(rootHash), blockHash);
                    var exists = await _blockStorage.ContainsKeyAsync(blockName, cancellationToken);
                    if (!exists) return false;
                }

                foreach (var blockHash in lastMerkleTreeSection.Hashes)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var blockName = ComputeBlockName(StringConverter.HashToString(rootHash), blockHash);
                    using var memoryOwner = await _blockStorage.TryReadAsync(blockName, cancellationToken);
                    if (memoryOwner is null) return false;

                    if (rootHash.Validate(memoryOwner.Memory.Span))
                    {
                        await _blockStorage.TryDeleteAsync(blockName, cancellationToken);
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
                if (decodedItem is null || decodedItem.MerkleTreeSections.Any(n => !n.Contains(blockHash))) return null;

                var blockName = ComputeBlockName(StringConverter.HashToString(rootHash), blockHash);
                return await _blockStorage.TryReadAsync(blockName, cancellationToken);
            }
        }

        public async ValueTask WriteBlockAsync(OmniHash rootHash, OmniHash blockHash, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                var decodedItem = _subscriberRepo.DecodedItems.FindOne(rootHash);
                if (decodedItem is null || decodedItem.MerkleTreeSections.Any(n => !n.Contains(blockHash))) return;

                var blockName = ComputeBlockName(StringConverter.HashToString(rootHash), blockHash);
                await _blockStorage.WriteAsync(blockName, memory, cancellationToken);
            }
        }

        private static string ComputeBlockName(string prefix, OmniHash blockHash)
        {
            return prefix + "/" + StringConverter.HashToString(blockHash);
        }
    }
}