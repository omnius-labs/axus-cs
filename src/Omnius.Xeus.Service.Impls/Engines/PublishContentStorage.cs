/*
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
using Omnius.Xeus.Service.Drivers;
using Omnius.Xeus.Service.Engines.Internal;

namespace Omnius.Xeus.Service.Engines
{
    public sealed class PublishContentStorage : AsyncDisposableBase, IPublishContentStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly PublishContentStorageOptions _options;
        private readonly IObjectStoreFactory _objectStoreFactory;
        private readonly IBytesPool _bytesPool;

        const int MaxBlockLength = 1 * 1024 * 1024;

        private readonly Dictionary<string, OmniHash> _publishFilePathToRootHashMap = new Dictionary<string, OmniHash>();
        private readonly Dictionary<OmniHash, PublishFileStatus> _publishFileStatusMap = new Dictionary<OmniHash, PublishFileStatus>();

        private readonly AsyncLock _asyncLock = new AsyncLock();

        internal sealed class PublishContentStorageFactory : IPublishContentStorageFactory
        {
            public async ValueTask<IPublishContentStorage> CreateAsync(PublishContentStorageOptions options,
                IObjectStoreFactory objectStoreFactory, IBytesPool bytesPool)
            {
                var result = new PublishContentStorage(options, objectStoreFactory, bytesPool);
                return result;
            }
        }

        public static IPublishContentStorageFactory Factory { get; } = new PublishContentStorageFactory();

        internal PublishContentStorage(PublishContentStorageOptions options, IObjectStoreFactory objectStoreFactory, IBytesPool bytesPool)
        {
            _options = options;
            _objectStoreFactory = objectStoreFactory;
            _bytesPool = bytesPool;
        }

        internal async ValueTask InitAsync()
        {
        }

        protected override async ValueTask OnDisposeAsync()
        {
        }

        public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
        }

        private string GenFilePath(OmniHash hash)
        {
            return hash.ToString(ConvertStringType.Base16, ConvertStringCase.Lower);
        }

        public async ValueTask<IMemoryOwner<byte>?> ReadAsync(OmniHash rootHash, OmniHash targetHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var filePath = Path.Combine(Path.Combine(_options.ConfigPath, this.GenFilePath(rootHash)), this.GenFilePath(targetHash));

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
            var filePath = Path.Combine(basePath, this.GenFilePath(hash));

            using (var fileStream = new UnbufferedFileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bytesPool))
            {
                await fileStream.WriteAsync(memory);
            }
        }

        public async ValueTask<PublishContentStorageReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<Tag> GetPublishTags()
        {
            throw new NotImplementedException();
        }

        public bool Contains(OmniHash rootHash)
        {
            throw new NotImplementedException();
        }

        public bool Contains(OmniHash rootHash, OmniHash targetHash)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<OmniHash> PublishContentAsync(string filePath, CancellationToken cancellationToken = default)
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
                    var tempPath = Path.Combine(_options.ConfigPath, "_temp_");

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
                        var cachePath = Path.Combine(_options.ConfigPath, this.GenFilePath(rootHash));
                        Directory.Move(tempPath, cachePath);
                    }

                    var status = new PublishFileStatus(rootHash, filePath, merkleTreeSections.ToArray());
                    _publishFileStatusMap.Add(rootHash, status);

                    _publishFilePathToRootHashMap[filePath] = rootHash;

                    return rootHash;
                }
            }
        }

        public async ValueTask UnpublishContentAsync(string filePath, CancellationToken cancellationToken = default)
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
                var cachePath = Path.Combine(_options.ConfigPath, this.GenFilePath(rootHash));
                Directory.Delete(cachePath);
            }
        }

        public ValueTask<OmniHash> PublishContentAsync(ReadOnlySequence<byte> sequence, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask UnpublishContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<OmniHash> PublishAsync(string filePath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<OmniHash> PublishAsync(ReadOnlySequence<byte> sequence, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask UnpublishAsync(string filePath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask UnpublishAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        IEnumerable<ResourceTag> IPublishStorage.GetPublishTags()
        {
            throw new NotImplementedException();
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
*/
