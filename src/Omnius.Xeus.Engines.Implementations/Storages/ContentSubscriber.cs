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
using Omnius.Core.Helpers;
using Omnius.Core.Io;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages.Internal;
using Omnius.Xeus.Engines.Storages.Internal.Models;
using Omnius.Xeus.Engines.Storages.Internal.Repositories;

namespace Omnius.Xeus.Engines.Storages
{
    public sealed partial class ContentSubscriber : AsyncDisposableBase, IContentSubscriber
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ContentSubscriberOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly ContentSubscriberRepository _subscriberRepo;
        private readonly BlockStogage _blockStorage;

        private Task _computeLoopTask = null!;

        private readonly AsyncReaderWriterLock _asyncLock = new();

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        internal sealed class ContentSubscriberFactory : IContentSubscriberFactory
        {
            public async ValueTask<IContentSubscriber> CreateAsync(ContentSubscriberOptions options, IBytesPool bytesPool)
            {
                var result = new ContentSubscriber(options, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IContentSubscriberFactory Factory { get; } = new ContentSubscriberFactory();

        private ContentSubscriber(ContentSubscriberOptions options, IBytesPool bytesPool)
        {
            _options = options;
            _bytesPool = bytesPool;

            _subscriberRepo = new ContentSubscriberRepository(Path.Combine(_options.ConfigDirectoryPath, "content_subscriber.db"));
            _blockStorage = new BlockStogage(Path.Combine(_options.ConfigDirectoryPath, "blocks.db"), _bytesPool);
        }

        internal async ValueTask InitAsync()
        {
            await _subscriberRepo.MigrateAsync();
            await _blockStorage.MigrateAsync();

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

                    await this.UpdateDecodedContentItemAsync(_cancellationTokenSource.Token);
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

        private async ValueTask UpdateDecodedContentItemAsync(CancellationToken cancellationToken = default)
        {
            var decodedItems = _subscriberRepo.DecodedItems.FindAll();

            foreach (var decodedItem in decodedItems)
            {
                var lastMerkleTreeSection = decodedItem.MerkleTreeSections[^1];
                if (lastMerkleTreeSection.Depth == 0)
                {
                    continue;
                }

                var completed = await this.ValidateDecodingCompletedAsync(decodedItem.ContentHash, lastMerkleTreeSection.Hashes, cancellationToken);
                if (!completed)
                {
                    continue;
                }

                var nextMerkleTreeSection = await this.DecodeMerkleTreeSectionAsync(decodedItem.ContentHash, lastMerkleTreeSection.Hashes, cancellationToken);
                if (nextMerkleTreeSection is null)
                {
                    continue;
                }

                lock (await _asyncLock.WriterLockAsync(cancellationToken))
                {
                    _subscriberRepo.DecodedItems.Insert(new DecodedContentItem(decodedItem.ContentHash, decodedItem.MerkleTreeSections.Append(nextMerkleTreeSection).ToArray()));
                }
            }
        }

        private async ValueTask<bool> ValidateDecodingCompletedAsync(OmniHash contentHash, IEnumerable<OmniHash> blockHashes, CancellationToken cancellationToken = default)
        {
            foreach (var blockHash in blockHashes)
            {
                var blockName = ComputeBlockName(StringConverter.HashToString(contentHash), blockHash);
                var exists = await _blockStorage.ExistsAsync(blockName, cancellationToken);

                if (!exists)
                {
                    return false;
                }
            }

            return true;
        }

        private async ValueTask<MerkleTreeSection?> DecodeMerkleTreeSectionAsync(OmniHash contentHash, IEnumerable<OmniHash> blockHashes, CancellationToken cancellationToken = default)
        {
            using var hub = new BytesHub();

            foreach (var blockHash in blockHashes)
            {
                var blockName = ComputeBlockName(StringConverter.HashToString(contentHash), blockHash);
                using var memoryOwner = await _blockStorage.ReadAsync(blockName, cancellationToken);
                if (memoryOwner is null)
                {
                    return null;
                }

                hub.Writer.Write(memoryOwner.Memory.Span);
            }

            return MerkleTreeSection.Import(hub.Reader.GetSequence(), _bytesPool);
        }

        public async ValueTask<ContentSubscriberReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var itemReports = new List<ContentSubscribedItemReport>();

                foreach (var item in _subscriberRepo.Items.FindAll())
                {
                    itemReports.Add(new ContentSubscribedItemReport(item.ContentHash, item.Registrant));
                }

                return new ContentSubscriberReport(itemReports.ToArray());
            }
        }

        public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
        }

        public async ValueTask<IEnumerable<OmniHash>> GetContentHashesAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var results = new List<OmniHash>();

                foreach (var status in _subscriberRepo.Items.FindAll())
                {
                    results.Add(status.ContentHash);
                }

                return results;
            }
        }

        public async ValueTask<IEnumerable<OmniHash>> GetBlockHashesAsync(OmniHash contentHash, bool? exists = null, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                if (exists.HasValue && !exists.Value)
                {
                    return Enumerable.Empty<OmniHash>();
                }

                if (!_subscriberRepo.Items.Exists(contentHash))
                {
                    return Enumerable.Empty<OmniHash>();
                }

                var decodedItem = _subscriberRepo.DecodedItems.FindOne(contentHash);
                if (decodedItem is null)
                {
                    return Enumerable.Empty<OmniHash>();
                }

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
                        var blockName = ComputeBlockName(StringConverter.HashToString(contentHash), blockHash);
                        if (await _blockStorage.ExistsAsync(blockName, cancellationToken) != exists.Value)
                        {
                            continue;
                        }

                        filteredHashes.Add(blockHash);
                    }

                    return filteredHashes;
                }
            }
        }

        public async ValueTask<bool> ContainsContentAsync(OmniHash contentHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                if (!_subscriberRepo.Items.Exists(contentHash))
                {
                    return false;
                }

                return true;
            }
        }

        public async ValueTask<bool> ContainsBlockAsync(OmniHash contentHash, OmniHash blockHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var decodedItem = _subscriberRepo.DecodedItems.FindOne(contentHash);
                if (decodedItem is null)
                {
                    return false;
                }

                return decodedItem.MerkleTreeSections.Any(n => n.Hashes.Contains(blockHash));
            }
        }

        public async ValueTask SubscribeContentAsync(OmniHash contentHash, string registrant, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                _subscriberRepo.Items.Insert(new SubscribedContentItem(contentHash, registrant));
            }
        }

        public async ValueTask UnsubscribeContentAsync(OmniHash contentHash, string registrant, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                _subscriberRepo.Items.Delete(contentHash, registrant);

                if (_subscriberRepo.Items.Exists(contentHash))
                {
                    return;
                }

                var decodedItem = _subscriberRepo.DecodedItems.FindOne(contentHash);
                if (decodedItem is null)
                {
                    return;
                }

                foreach (var blockHash in decodedItem.MerkleTreeSections.SelectMany(n => n.Hashes))
                {
                    var blockName = ComputeBlockName(StringConverter.HashToString(contentHash), blockHash);
                    await _blockStorage.DeleteAsync(blockName, cancellationToken);
                }

                _subscriberRepo.DecodedItems.Delete(contentHash);
            }
        }

        public async ValueTask ExportContentAsync(OmniHash contentHash, string filePath, CancellationToken cancellationToken = default)
        {
            using var fileStream = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None, _bytesPool);
            var writer = PipeWriter.Create(fileStream);
            await this.ExportContentAsync(contentHash, writer, cancellationToken);
            await writer.CompleteAsync();
        }

        public async ValueTask ExportContentAsync(OmniHash contentHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var decodedItem = _subscriberRepo.DecodedItems.FindOne(contentHash);
                if (decodedItem is null)
                {
                    // FIXME
                    throw new KeyNotFoundException();
                }

                var lastMerkleTreeSection = decodedItem.MerkleTreeSections[^1];
                if (lastMerkleTreeSection.Depth != 0)
                {
                    // FIXME
                    throw new KeyNotFoundException();
                }

                foreach (var blockHash in lastMerkleTreeSection.Hashes)
                {
                    var blockName = ComputeBlockName(StringConverter.HashToString(contentHash), blockHash);
                    var exists = await _blockStorage.ExistsAsync(blockName, cancellationToken);

                    if (!exists)
                    {
                        // FIXME
                        throw new KeyNotFoundException();
                    }
                }

                foreach (var blockHash in lastMerkleTreeSection.Hashes)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var blockName = ComputeBlockName(StringConverter.HashToString(contentHash), blockHash);
                    using var memoryOwner = await _blockStorage.ReadAsync(blockName, cancellationToken);

                    if (memoryOwner is null)
                    {
                        // FIXME
                        throw new KeyNotFoundException();
                    }

                    bufferWriter.Write(memoryOwner.Memory.Span);
                    bufferWriter.Advance(memoryOwner.Memory.Length);
                }
            }
        }

        public async ValueTask<IMemoryOwner<byte>?> ReadBlockAsync(OmniHash contentHash, OmniHash blockHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var decodedItem = _subscriberRepo.DecodedItems.FindOne(contentHash);
                if (decodedItem is null || decodedItem.MerkleTreeSections.Any(n => !n.Contains(blockHash)))
                {
                    return null;
                }

                var blockName = ComputeBlockName(StringConverter.HashToString(contentHash), blockHash);
                return await _blockStorage.ReadAsync(blockName, cancellationToken);
            }
        }

        public async ValueTask WriteBlockAsync(OmniHash contentHash, OmniHash blockHash, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                var decodedItem = _subscriberRepo.DecodedItems.FindOne(contentHash);
                if (decodedItem is null || decodedItem.MerkleTreeSections.Any(n => !n.Contains(blockHash)))
                {
                    return;
                }

                var blockName = ComputeBlockName(StringConverter.HashToString(contentHash), blockHash);
                await _blockStorage.WriteAsync(blockName, memory, cancellationToken);
            }
        }

        private static string ComputeBlockName(string prefix, OmniHash blockHash)
        {
            return prefix + "/" + StringConverter.HashToString(blockHash);
        }
    }
}
