using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
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
        private readonly Dictionary<string, OmniHash> _publishFilePathToRootHashMap = new Dictionary<string, OmniHash>();
        private readonly Dictionary<OmniHash, PublishFileStatus> _publishFileStatusMap = new Dictionary<OmniHash, PublishFileStatus>();

        const int MaxBlockLength = 1 * 1024 * 1024;

        private string GetTempPath()
        {
            return Path.Combine(_configPath, "_temp_");
        }

        private string OmniHashToString(OmniHash hash)
        {
            using var hub = new Hub(_bufferPool);
            hash.Export(hub.Writer, _bufferPool);

            var value = OmniBase.ToBase58BtcString(hub.Reader.GetSequence());

            return Path.Combine(_configPath, value);
        }

        private void WriteBlock(string basePath, OmniHash hash)
        {
            var blockFilePath = Path.Combine(directoryPath, this.OmniHashToString(hash));

            using (var fileStream = new UnbufferedFileStream(blockFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bufferPool))
            {
                await fileStream.WriteAsync(memory, cancellationToken);
            }
        }

        public async ValueTask<OmniHash> AddPublishFile(string filePath, CancellationToken cancellationToken = default)
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
                    var directoryPath = this.GetTempPath();

                    var merkleTreeSections = new Stack<MerkleTreeSection>();

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

                                hashList.Add(new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memory.Span)));
                            }
                        }

                        merkleTreeSections.Push(new MerkleTreeSection((ulong)inStream.Length, hashList.ToArray()));
                    }

                    OmniHash rootHash;

                    for (; ; )
                    {
                        using var hub = new Hub(_bufferPool);

                        var lastMerkleTreeSection = merkleTreeSections.Peek();
                        lastMerkleTreeSection.Export(hub.Writer, _bufferPool);

                        if (hub.Writer.WrittenBytes > MaxBlockLength)
                        {
                            var sequence = hub.Reader.GetSequence();

                            var hashList = new List<OmniHash>();

                            using (var memoryOwner = _bufferPool.RentMemory(MaxBlockLength))
                            {
                                var remain = sequence.Length;

                                while (remain > 0)
                                {
                                    var blockLength = (int)Math.Min(remain, MaxBlockLength);
                                    remain -= blockLength;

                                    var memory = memoryOwner.Memory.Slice(0, blockLength);
                                    sequence.CopyTo(memory.Span);

                                    var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(memory.Span));
                                    hashList.Add(hash);

                                    var blockFilePath = Path.Combine(directoryPath, this.OmniHashToString(hash));
                                    using (var fileStream = new UnbufferedFileStream(blockFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bufferPool))
                                    {
                                        await fileStream.WriteAsync(memory, cancellationToken);
                                    }

                                    sequence = sequence.Slice(blockLength);
                                }
                            }

                            merkleTreeSections.Push(new MerkleTreeSection((ulong)hub.Writer.WrittenBytes, hashList.ToArray()));
                        }
                        else
                        {
                            rootHash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(sequence));

                            break;
                        }
                    }

                    var status = new PublishFileStatus(rootHash, filePath, merkleTreeSections.ToArray());
                    _publishFileStatusMap.Add(rootHash, status);

                    _publishFilePathToRootHashMap[filePath] = rootHash;

                    return rootHash;
                }
            }
        }

        public void RemovePublishFile(string path)
        {

        }

        public IEnumerable<PublishFileReport> GetPublishFileReports()
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
