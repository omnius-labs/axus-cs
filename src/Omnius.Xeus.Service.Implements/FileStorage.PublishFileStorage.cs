using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Io;
using Omnius.Core.Serialization;
using Omnius.Xeus.Service.Internal;

namespace Omnius.Xeus.Service
{
    public sealed partial class FileStorage
    {
        private sealed class PublishFileStorage
        {
            private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

            private readonly string _configPath;
            private readonly IBufferPool<byte> _bufferPool;

            private readonly Dictionary<string, OmniHash> _publishFilePathToRootHashMap = new Dictionary<string, OmniHash>();
            private readonly Dictionary<OmniHash, PublishFileStatus> _publishFileStatusMap = new Dictionary<OmniHash, PublishFileStatus>();

            private readonly AsyncLock _asyncLock = new AsyncLock();

            public PublishFileStorage(string configPath, IBufferPool<byte> bufferPool)
            {
                _configPath = configPath;
                _bufferPool = bufferPool;
            }

            private string OmniHashToString(OmniHash hash)
            {
                using var hub = new Hub(_bufferPool);
                hash.Export(hub.Writer, _bufferPool);

                var value = OmniBase.ToBase58BtcString(hub.Reader.GetSequence());

                return Path.Combine(_configPath, value);
            }

            public async ValueTask<IMemoryOwner<byte>?> ReadAsync(OmniHash rootHash, OmniHash targetHash, CancellationToken cancellationToken = default)
            {
                using (await _asyncLock.LockAsync())
                {
                    var filePath = Path.Combine(Path.Combine(_configPath, this.OmniHashToString(rootHash)), this.OmniHashToString(targetHash));

                    if (!File.Exists(filePath))
                    {
                        return null;
                    }

                    using (var fileStream = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None, _bufferPool))
                    {
                        var memoryOwner = _bufferPool.RentMemory((int)fileStream.Length);
                        await fileStream.ReadAsync(memoryOwner.Memory);

                        return memoryOwner;
                    }
                }
            }

            private async ValueTask WriteAsync(string basePath, OmniHash hash, ReadOnlyMemory<byte> memory)
            {
                var filePath = Path.Combine(basePath, this.OmniHashToString(hash));

                using (var fileStream = new UnbufferedFileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bufferPool))
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

                            using (var memoryOwner = _bufferPool.RentMemory(MaxBlockLength))
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
                            using var hub = new Hub(_bufferPool);

                            var lastMerkleTreeSection = merkleTreeSections.Peek();
                            lastMerkleTreeSection.Export(hub.Writer, _bufferPool);

                            if (hub.Writer.WrittenBytes > MaxBlockLength)
                            {
                                var hashList = new List<OmniHash>();

                                using (var memoryOwner = _bufferPool.RentMemory(MaxBlockLength))
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
                                using (var memoryOwner = _bufferPool.RentMemory(MaxBlockLength))
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
                            var cachePath = Path.Combine(_configPath, this.OmniHashToString(rootHash));
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
                    var cachePath = Path.Combine(_configPath, this.OmniHashToString(rootHash));
                    Directory.Delete(cachePath);
                }
            }

            public async IAsyncEnumerable<PublishFileReport> GetPublishFileReportsAsync([EnumeratorCancellation]CancellationToken cancellationToken = default)
            {
                using (await _asyncLock.LockAsync())
                {
                    foreach (var status in _publishFileStatusMap.Values)
                    {
                        yield return new PublishFileReport(status.RootHash, status.FilePath);
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
}
