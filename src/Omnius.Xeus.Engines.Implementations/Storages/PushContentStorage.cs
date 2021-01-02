using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Cryptography.Functions;
using Omnius.Core.Extensions;
using Omnius.Core.Io;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages.Internal.Models;
using Omnius.Xeus.Engines.Storages.Internal.Repositories;

namespace Omnius.Xeus.Engines.Storages
{
    public sealed partial class PushContentStorage : AsyncDisposableBase, IPushContentStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly PushContentStorageOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly PushContentStorageRepository _repository;

        private const int MaxBlockLength = 32 * 1024 * 1024;

        internal sealed class PushContentStorageFactory : IPushContentStorageFactory
        {
            public async ValueTask<IPushContentStorage> CreateAsync(PushContentStorageOptions options, IBytesPool bytesPool)
            {
                var result = new PushContentStorage(options, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IPushContentStorageFactory Factory { get; } = new PushContentStorageFactory();

        private PushContentStorage(PushContentStorageOptions options, IBytesPool bytesPool)
        {
            _options = options;
            _bytesPool = bytesPool;

            _repository = new PushContentStorageRepository(Path.Combine(_options.ConfigDirectoryPath, "push-content-storage.db"));
        }

        internal async ValueTask InitAsync()
        {
            await _repository.MigrateAsync();
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _repository.Dispose();
        }

        public async ValueTask<PushContentStorageReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            var pushContentReports = new List<PushContentReport>();

            foreach (var status in _repository.PushContentStatus.GetAll())
            {
                pushContentReports.Add(new PushContentReport(status.FilePath, status.Hash));
            }

            return new PushContentStorageReport(pushContentReports.ToArray());
        }

        public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
        }

        public async ValueTask<IEnumerable<OmniHash>> GetContentHashesAsync(CancellationToken cancellationToken = default)
        {
            var results = new List<OmniHash>();

            foreach (var status in _repository.PushContentStatus.GetAll())
            {
                results.Add(status.Hash);
            }

            return results;
        }

        public async ValueTask<IEnumerable<OmniHash>> GetBlockHashesAsync(OmniHash contentHash, bool? exists = null, CancellationToken cancellationToken = default)
        {
            if (exists.HasValue && !exists.Value)
            {
                return Enumerable.Empty<OmniHash>();
            }

            var status = _repository.PushContentStatus.Get(contentHash);
            if (status == null)
            {
                return Enumerable.Empty<OmniHash>();
            }

            return status.MerkleTreeSections.SelectMany(n => n.Hashes);
        }

        public async ValueTask<bool> ContainsContentAsync(OmniHash contentHash)
        {
            var status = _repository.PushContentStatus.Get(contentHash);
            if (status == null)
            {
                return false;
            }

            return true;
        }

        public async ValueTask<bool> ContainsBlockAsync(OmniHash contentHash, OmniHash blockHash)
        {
            var status = _repository.PushContentStatus.Get(contentHash);
            if (status == null)
            {
                return false;
            }

            return status.MerkleTreeSections.Any(n => n.Contains(contentHash));
        }

        public async ValueTask<OmniHash> RegisterPushContentAsync(string filePath, CancellationToken cancellationToken = default)
        {
            // 既にエンコード済みの場合
            {
                var status = _repository.PushContentStatus.Get(filePath);
                if (status != null)
                {
                    return status.Hash;
                }
            }

            // エンコード処理
            {
                var tempDirPath = Path.Combine(_options.ConfigDirectoryPath, "cache", "_temp_");

                var merkleTreeSections = new Stack<MerkleTreeSection>();

                // ファイルからハッシュ値を算出する
                using (var inStream = new FileStream(filePath, FileMode.Open))
                {
                    var hashList = new List<OmniHash>();

                    using (var memoryOwner = _bytesPool.Memory.Rent(MaxBlockLength))
                    {
                        var remain = inStream.Length;

                        while (remain > 0)
                        {
                            var blockLength = (int)Math.Min(remain, MaxBlockLength);
                            remain -= blockLength;

                            var memory = memoryOwner.Memory.Slice(0, blockLength);
                            inStream.Read(memory.Span);

                            var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memory.Span));
                            hashList.Add(hash);
                        }
                    }

                    merkleTreeSections.Push(new MerkleTreeSection(0, MaxBlockLength, (ulong)inStream.Length, hashList.ToArray()));
                }

                OmniHash contentHash;

                // ハッシュ値からMerkle treeを作成する
                for (; ; )
                {
                    using var hub = new BytesHub(_bytesPool);

                    var lastMerkleTreeSection = merkleTreeSections.Peek();
                    lastMerkleTreeSection.Export(hub.Writer, _bytesPool);

                    if (hub.Writer.WrittenBytes > MaxBlockLength)
                    {
                        var hashList = new List<OmniHash>();

                        using (var memoryOwner = _bytesPool.Memory.Rent(MaxBlockLength))
                        {
                            var sequence = hub.Reader.GetSequence();
                            var remain = sequence.Length;

                            while (remain > 0)
                            {
                                var blockLength = (int)Math.Min(remain, MaxBlockLength);
                                remain -= blockLength;

                                var memory = memoryOwner.Memory.Slice(0, blockLength);
                                sequence.CopyTo(memory.Span);

                                var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memory.Span));
                                hashList.Add(hash);

                                await this.WriteBlockAsync(tempDirPath, hash, memory);

                                sequence = sequence.Slice(blockLength);
                            }
                        }

                        merkleTreeSections.Push(new MerkleTreeSection(merkleTreeSections.Count, MaxBlockLength, (ulong)hub.Writer.WrittenBytes, hashList.ToArray()));
                    }
                    else
                    {
                        using var memoryOwner = _bytesPool.Memory.Rent(MaxBlockLength);
                        var sequence = hub.Reader.GetSequence();

                        var memory = memoryOwner.Memory.Slice(0, (int)sequence.Length);
                        sequence.CopyTo(memory.Span);

                        var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memory.Span));

                        await this.WriteBlockAsync(tempDirPath, hash, memory);

                        contentHash = hash;

                        break;
                    }
                }

                // 一時フォルダからキャッシュフォルダへ移動させる
                {
                    var cacheDirPath = this.ComputeCacheDirectoryPath(contentHash);
                    Directory.Move(tempDirPath, cacheDirPath);
                }

                var status = new PushContentStatus(contentHash, filePath, merkleTreeSections.ToArray());
                _repository.PushContentStatus.Add(status);

                return contentHash;
            }
        }

        private async ValueTask WriteBlockAsync(string basePath, OmniHash hash, ReadOnlyMemory<byte> memory)
        {
            var filePath = Path.Combine(basePath, StringConverter.HashToString(hash));
            using var fileStream = new UnbufferedFileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bytesPool);
            await fileStream.WriteAsync(memory);
        }

        public async ValueTask UnregisterPushContentAsync(string filePath, CancellationToken cancellationToken = default)
        {
            var status = _repository.PushContentStatus.Get(filePath);
            if (status == null)
            {
                return;
            }

            _repository.PushContentStatus.Remove(filePath);

            // キャッシュフォルダを削除
            var cacheDirPath = this.ComputeCacheDirectoryPath(status.Hash);
            Directory.Delete(cacheDirPath);
        }

        public async ValueTask<IMemoryOwner<byte>?> ReadBlockAsync(OmniHash contentHash, OmniHash blockHash, CancellationToken cancellationToken = default)
        {
            var status = _repository.PushContentStatus.Get(contentHash);
            if (status == null)
            {
                return null;
            }

            var lastMerkleTreeSections = status.MerkleTreeSections[^1];
            var middleMerkleTreeSections = status.MerkleTreeSections.ToArray()[..^1];

            if (lastMerkleTreeSections.TryGetIndex(blockHash, out var index))
            {
                if (!File.Exists(status.FilePath))
                {
                    return null;
                }

                var position = lastMerkleTreeSections.BlockLength * index;
                var blockSize = (int)(lastMerkleTreeSections.Length - (ulong)position);

                using var fileStream = new UnbufferedFileStream(status.FilePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None, _bytesPool);
                fileStream.Seek(position, SeekOrigin.Begin);
                var memoryOwner = _bytesPool.Memory.Rent(blockSize).Shrink(blockSize);
                await fileStream.ReadAsync(memoryOwner.Memory, cancellationToken);

                return memoryOwner;
            }
            else if (middleMerkleTreeSections.Any(n => n.Contains(blockHash)))
            {
                string filePath = this.ComputeCacheFilePath(contentHash, blockHash);
                if (!File.Exists(filePath))
                {
                    return null;
                }

                using var fileStream = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None, _bytesPool);
                var memoryOwner = _bytesPool.Memory.Rent((int)fileStream.Length);
                await fileStream.ReadAsync(memoryOwner.Memory, cancellationToken);

                return memoryOwner;
            }

            return null;
        }

        private string ComputeCacheDirectoryPath(OmniHash contentHash)
        {
            return Path.Combine(_options.ConfigDirectoryPath, "cache", StringConverter.HashToString(contentHash));
        }

        private string ComputeCacheFilePath(OmniHash contentHash, OmniHash blockHash)
        {
            return Path.Combine(this.ComputeCacheDirectoryPath(contentHash), StringConverter.HashToString(blockHash));
        }
    }
}
