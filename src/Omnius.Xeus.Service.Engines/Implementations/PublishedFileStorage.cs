using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Cryptography.Functions;
using Omnius.Core.Pipelines;
using Omnius.Core.Storages;
using Omnius.Core.Streams;
using Omnius.Xeus.Service.Engines.Internal;
using Omnius.Xeus.Service.Engines.Internal.Models;
using Omnius.Xeus.Service.Engines.Internal.Repositories;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines
{
    public sealed partial class PublishedFileStorage : AsyncDisposableBase, IPublishedFileStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IBytesStorageFactory _bytesStorageFactory;
        private readonly IBytesPool _bytesPool;
        private readonly PublishedFileStorageOptions _options;

        private readonly PublishedFileStorageRepository _publisherRepo;
        private readonly IBytesStorage<string> _blockStorage;

        private readonly AsyncReaderWriterLock _asyncLock = new();

        private const int MaxBlockLength = 8 * 1024 * 1024;

        public PublishedFileStorage(IBytesStorageFactory bytesStorageFactory, IBytesPool bytesPool, PublishedFileStorageOptions options)
        {
            _bytesStorageFactory = bytesStorageFactory;
            _bytesPool = bytesPool;
            _options = options;

            _publisherRepo = new PublishedFileStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "state"));
            _blockStorage = _bytesStorageFactory.Create<string>(Path.Combine(_options.ConfigDirectoryPath, "blocks"), _bytesPool);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _publisherRepo.Dispose();
            _blockStorage.Dispose();
        }

        public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            await _publisherRepo.MigrateAsync(cancellationToken);
            await _blockStorage.MigrateAsync(cancellationToken);
        }

        public async ValueTask<PublishedFileStorageReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var fileReports = new List<PublishedFileReport>();

                foreach (var item in _publisherRepo.Items.FindAll())
                {
                    fileReports.Add(new PublishedFileReport(item.FilePath, item.RootHash, item.Registrant));
                }

                return new PublishedFileStorageReport(fileReports.ToArray());
            }
        }

        public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<IEnumerable<OmniHash>> GetRootHashesAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var results = new List<OmniHash>();

                foreach (var item in _publisherRepo.Items.FindAll())
                {
                    results.Add(item.RootHash);
                }

                return results;
            }
        }

        public async ValueTask<IEnumerable<OmniHash>> GetBlockHashesAsync(OmniHash rootHash, bool? exists = null, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                if (exists.HasValue && !exists.Value) return Enumerable.Empty<OmniHash>();

                var item = _publisherRepo.Items.Find(rootHash).FirstOrDefault();
                if (item is null) return Enumerable.Empty<OmniHash>();

                return item.MerkleTreeSections.SelectMany(n => n.Hashes);
            }
        }

        public async ValueTask<bool> ContainsFileAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                if (!_publisherRepo.Items.Exists(rootHash)) return false;

                return true;
            }
        }

        public async ValueTask<bool> ContainsBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var item = _publisherRepo.Items.Find(rootHash).FirstOrDefault();
                if (item is null) return false;

                return item.MerkleTreeSections.Any(n => n.Contains(rootHash));
            }
        }

        public async ValueTask<OmniHash> PublishFileAsync(string filePath, string registrant, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var item = _publisherRepo.Items.FindOne(filePath, registrant);
                if (item is not null) return item.RootHash;
            }

            {
                var tempPrefix = "_temp_" + this.GenUniqueId();

                var lastMerkleTreeSection = await this.EncodeFileAsync(filePath, cancellationToken);
                var (rootHash, middleMerkleTreeSections) = await this.EncodeMerkleTreeSectionsAsync(lastMerkleTreeSection, tempPrefix, cancellationToken);

                var mergedMerkleTreeSections = middleMerkleTreeSections.Append(lastMerkleTreeSection).ToArray();
                var item = new PublishedFileItem(rootHash, filePath, registrant, mergedMerkleTreeSections);

                using (await _asyncLock.WriterLockAsync(cancellationToken))
                {
                    // FIXME
                    // 途中で処理が中断された場合に残骸となったブロックを除去する処理が必要
                    await this.RenameBlocksAsync(tempPrefix, StringConverter.HashToString(rootHash), middleMerkleTreeSections.SelectMany(n => n.Hashes), cancellationToken);

                    _publisherRepo.Items.Upsert(item);
                }

                return rootHash;
            }
        }

        public async ValueTask<OmniHash> PublishFileAsync(ReadOnlySequence<byte> sequence, string registrant, CancellationToken cancellationToken = default)
        {
            var tempPrefix = "_temp_" + this.GenUniqueId();

            var lastMerkleTreeSection = await this.EncodeMemoryAsync(sequence, tempPrefix, cancellationToken);
            var (rootHash, middleMerkleTreeSections) = await this.EncodeMerkleTreeSectionsAsync(lastMerkleTreeSection, tempPrefix, cancellationToken);

            var mergedMerkleTreeSections = middleMerkleTreeSections.Append(lastMerkleTreeSection).ToArray();
            var item = new PublishedFileItem(rootHash, null, registrant, mergedMerkleTreeSections);

            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                // FIXME
                // 途中で処理が中断された場合に残骸となったブロックを除去する処理が必要
                await this.RenameBlocksAsync(tempPrefix, StringConverter.HashToString(rootHash), mergedMerkleTreeSections.SelectMany(n => n.Hashes), cancellationToken);

                _publisherRepo.Items.Upsert(item);
            }

            return rootHash;
        }

        private async ValueTask RenameBlocksAsync(string oldPrefix, string newPrefix, IEnumerable<OmniHash> blockHashes, CancellationToken cancellationToken = default)
        {
            foreach (var blockHash in blockHashes.ToHashSet())
            {
                var oldName = ComputeBlockName(oldPrefix, blockHash);
                var newName = ComputeBlockName(newPrefix, blockHash);
                await _blockStorage.ChangeKeyAsync(oldName, newName, cancellationToken);
            }
        }

        private async ValueTask<MerkleTreeSection> EncodeFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            // ファイルからハッシュ値を算出する
            using var inStream = new FileStream(filePath, FileMode.Open);

            var blockHashes = new List<OmniHash>();

            using (var memoryOwner = _bytesPool.Memory.Rent(MaxBlockLength))
            {
                var remain = inStream.Length;

                while (remain > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var blockLength = (int)Math.Min(remain, MaxBlockLength);
                    remain -= blockLength;

                    var memory = memoryOwner.Memory.Slice(0, blockLength);
                    await inStream.ReadAsync(memory, cancellationToken);

                    var blockHash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memory.Span));
                    blockHashes.Add(blockHash);
                }
            }

            return new MerkleTreeSection(0, MaxBlockLength, (ulong)inStream.Length, blockHashes.ToArray());
        }

        private async ValueTask<MerkleTreeSection> EncodeMemoryAsync(ReadOnlySequence<byte> sequence, string blockNamePrefix, CancellationToken cancellationToken = default)
        {
            var blockHashes = new List<OmniHash>();
            var sequenceLength = sequence.Length;

            using var memoryOwner = _bytesPool.Memory.Rent(MaxBlockLength);

            while (sequence.Length > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var blockLength = (int)Math.Min(sequence.Length, MaxBlockLength);

                var memory = memoryOwner.Memory.Slice(0, blockLength);
                sequence.CopyTo(memory.Span);
                sequence = sequence.Slice(blockLength);

                var blockHash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memory.Span));
                await this.WriteBlockAsync(blockNamePrefix, blockHash, memory);
                blockHashes.Add(blockHash);
            }

            return new MerkleTreeSection(0, MaxBlockLength, (ulong)sequenceLength, blockHashes.ToArray());
        }

        private async ValueTask<(OmniHash, IEnumerable<MerkleTreeSection>)> EncodeMerkleTreeSectionsAsync(MerkleTreeSection lastMerkleTreeSection, string blockNamePrefix, CancellationToken cancellationToken = default)
        {
            var resultMerkleTreeSections = new Stack<MerkleTreeSection>();

            // ハッシュ値からMerkle treeを作成する
            for (; ; )
            {
                using var bytesPool = new BytesPipe(_bytesPool);

                lastMerkleTreeSection.Export(bytesPool.Writer, _bytesPool);

                if (bytesPool.Writer.WrittenBytes > MaxBlockLength)
                {
                    var hashList = new List<OmniHash>();

                    using (var memoryOwner = _bytesPool.Memory.Rent(MaxBlockLength))
                    {
                        var sequence = bytesPool.Reader.GetSequence();
                        var remain = sequence.Length;

                        while (remain > 0)
                        {
                            var blockLength = (int)Math.Min(remain, MaxBlockLength);
                            remain -= blockLength;

                            var memory = memoryOwner.Memory.Slice(0, blockLength);
                            sequence.CopyTo(memory.Span);

                            var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memory.Span));
                            hashList.Add(hash);

                            await this.WriteBlockAsync(blockNamePrefix, hash, memory);

                            sequence = sequence.Slice(blockLength);
                        }
                    }

                    var newMerkleTreeSection = new MerkleTreeSection(lastMerkleTreeSection.Depth + 1, MaxBlockLength, (ulong)bytesPool.Writer.WrittenBytes, hashList.ToArray());
                    lastMerkleTreeSection = newMerkleTreeSection;
                    resultMerkleTreeSections.Push(newMerkleTreeSection);
                }
                else
                {
                    using var memoryOwner = _bytesPool.Memory.Rent(MaxBlockLength);
                    var sequence = bytesPool.Reader.GetSequence();

                    var memory = memoryOwner.Memory.Slice(0, (int)sequence.Length);
                    sequence.CopyTo(memory.Span);

                    var rootHash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memory.Span));

                    await this.WriteBlockAsync(blockNamePrefix, rootHash, memory);

                    return (rootHash, resultMerkleTreeSections.ToArray());
                }
            }
        }

        private async ValueTask WriteBlockAsync(string blockNamePrefix, OmniHash blockHash, ReadOnlyMemory<byte> memory)
        {
            var blockName = ComputeBlockName(blockNamePrefix, blockHash);
            await _blockStorage.WriteAsync(blockName, memory);
        }

        public async ValueTask UnpublishFileAsync(string filePath, string registrant, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
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
            using (await _asyncLock.WriterLockAsync(cancellationToken))
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
                var blockName = ComputeBlockName(StringConverter.HashToString(rootHash), blockHash);
                await _blockStorage.TryDeleteAsync(blockName);
            }
        }

        public async ValueTask<IMemoryOwner<byte>?> ReadBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var item = _publisherRepo.Items.Find(rootHash).FirstOrDefault();
                if (item is null || item.MerkleTreeSections.Any(n => !n.Contains(blockHash))) return null;

                if (item.FilePath is not null)
                {
                    var lastMerkleTreeSections = item.MerkleTreeSections[^1];
                    if (lastMerkleTreeSections.TryGetIndex(blockHash, out var index))
                    {
                        if (!File.Exists(item.FilePath)) return null;

                        var position = lastMerkleTreeSections.BlockLength * index;
                        var blockSize = (int)Math.Min(lastMerkleTreeSections.BlockLength, (int)(lastMerkleTreeSections.Length - (ulong)position));

                        using var fileStream = new UnbufferedFileStream(item.FilePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None, _bytesPool);
                        fileStream.Seek(position, SeekOrigin.Begin);
                        var memoryOwner = _bytesPool.Memory.Rent(blockSize).Shrink(blockSize);
                        await fileStream.ReadAsync(memoryOwner.Memory, cancellationToken);

                        return memoryOwner;
                    }
                }

                var blockName = ComputeBlockName(StringConverter.HashToString(rootHash), blockHash);
                return await _blockStorage.TryReadAsync(blockName, cancellationToken);
            }
        }

        private ulong _uniqueId = 0;

        private string GenUniqueId()
        {
            return Interlocked.Increment(ref _uniqueId).ToString();
        }

        private static string ComputeBlockName(string prefix, OmniHash blockHash)
        {
            return prefix + "/" + StringConverter.HashToString(blockHash);
        }
    }
}
