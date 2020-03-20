using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Io;
using Omnius.Core.Serialization;
using Omnius.Xeus.Service.Internal;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public sealed class PublishFileStorage : AsyncDisposableBase, IPublishFileStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _configPath;
        private readonly IBytesPool _bytesPool;

        const int MaxBlockLength = 1 * 1024 * 1024;

        private readonly Dictionary<string, OmniHash> _publishFilePathToRootHashMap = new Dictionary<string, OmniHash>();
        private readonly Dictionary<OmniHash, PublishFileStatus> _publishFileStatusMap = new Dictionary<OmniHash, PublishFileStatus>();

        private readonly AsyncLock _asyncLock = new AsyncLock();

        internal sealed class PublishFileStorageFactory : IPublishFileStorageFactory
        {
            public async ValueTask<IPublishFileStorage> CreateAsync(string configPath, IBytesPool bytesPool)
            {
                var result = new PublishFileStorage(configPath, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IPublishFileStorageFactory Factory { get; } = new PublishFileStorageFactory();

        internal PublishFileStorage(string configPath, IBytesPool bytesPool)
        {
            _configPath = configPath;
            _bytesPool = bytesPool;
        }

        public async ValueTask InitAsync()
        {
        }

        protected override async ValueTask OnDisposeAsync()
        {
        }

        public async ValueTask CheckConsistencyAsync(Action<CheckConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
        }

        private string OmniHashToFilePath(OmniHash hash)
        {
            return Path.Combine(_configPath, hash.ToString(ConvertStringType.Base16, ConvertStringCase.Lower));
        }

        public async ValueTask<IMemoryOwner<byte>?> ReadAsync(OmniHash rootHash, OmniHash targetHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var filePath = Path.Combine(Path.Combine(_configPath, this.OmniHashToFilePath(rootHash)), this.OmniHashToFilePath(targetHash));

                if (!File.Exists(filePath))
                {
                    return null;
                }

                using (var fileStream = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None, _bytesPool))
                {
                    var memoryOwner = _bytesPool.Memory.Rent((int)fileStream.Length);
                    await fileStream.ReadAsync(memoryOwner.Memory);

                    return memoryOwner;
                }
            }
        }

        private async ValueTask WriteAsync(string basePath, OmniHash hash, ReadOnlyMemory<byte> memory)
        {
            var filePath = Path.Combine(basePath, this.OmniHashToFilePath(hash));

            using (var fileStream = new UnbufferedFileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bytesPool))
            {
                await fileStream.WriteAsync(memory);
            }
        }

        public async ValueTask<OmniHash> AddPublishFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                // 既にエンコード済みの場合の処理
                {
                    if (_publishFilePathToRootHashMap.TryGetValue(filePath, out var rootHash))
                    {
                        return rootHash;
                    }
                }

                // エンコード処理
                {
                    var tempPath = Path.Combine(_configPath, "_temp_");

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

                        merkleTreeSections.Push(new MerkleTreeSection(0, (ulong)inStream.Length, hashList.ToArray()));
                    }

                    OmniHash rootHash;

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

                                    await this.WriteAsync(tempPath, hash, memory);

                                    sequence = sequence.Slice(blockLength);
                                }
                            }

                            merkleTreeSections.Push(new MerkleTreeSection(merkleTreeSections.Count, (ulong)hub.Writer.WrittenBytes, hashList.ToArray()));
                        }
                        else
                        {
                            using (var memoryOwner = _bytesPool.Memory.Rent(MaxBlockLength))
                            {
                                var sequence = hub.Reader.GetSequence();

                                var memory = memoryOwner.Memory.Slice(0, (int)sequence.Length);
                                sequence.CopyTo(memory.Span);

                                var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memory.Span));

                                await this.WriteAsync(tempPath, hash, memory);

                                rootHash = hash;
                            }

                            break;
                        }
                    }

                    // 一時フォルダからキャッシュフォルダへ移動させる
                    {
                        var cachePath = Path.Combine(_configPath, this.OmniHashToFilePath(rootHash));
                        Directory.Move(tempPath, cachePath);
                    }

                    var status = new PublishFileStatus(rootHash, filePath, merkleTreeSections.ToArray());
                    _publishFileStatusMap.Add(rootHash, status);

                    _publishFilePathToRootHashMap[filePath] = rootHash;

                    return rootHash;
                }
            }
        }

        public async ValueTask RemovePublishFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                if (!_publishFilePathToRootHashMap.TryGetValue(filePath, out var rootHash))
                {
                    return;
                }

                _publishFilePathToRootHashMap.Remove(filePath);

                _publishFileStatusMap.Remove(rootHash);

                // キャッシュフォルダを削除
                var cachePath = Path.Combine(_configPath, this.OmniHashToFilePath(rootHash));
                Directory.Delete(cachePath);
            }
        }

        public async IAsyncEnumerable<PublishReport> GetReportsAsync([EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                foreach (var status in _publishFileStatusMap.Values)
                {
                    yield return new PublishReport(status.RootHash, status.MerkleTreeSections.SelectMany(n => n.Hashes).ToArray());
                }
            }
        }

        class PublishFileStatus
        {
            public PublishFileStatus(OmniHash rootHash, string filePath, MerkleTreeSection[] merkleTreeSections)
            {
                this.FilePath = filePath;
                this.RootHash = rootHash;
                this.MerkleTreeSections = new ReadOnlyListSlim<MerkleTreeSection>(merkleTreeSections);
            }

            public string FilePath { get; }
            public OmniHash RootHash { get; }
            public ReadOnlyListSlim<MerkleTreeSection> MerkleTreeSections { get; }
        }
    }
}
