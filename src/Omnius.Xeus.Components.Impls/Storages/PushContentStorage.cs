using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Io;
using Omnius.Core.Serialization;
using Omnius.Xeus.Components.Engines.Internal;
using Omnius.Xeus.Components.Models;
using Omnius.Xeus.Components.Storages.Internal;

namespace Omnius.Xeus.Components.Storages
{
    public sealed partial class PushContentStorage : AsyncDisposableBase, IPushContentStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly PushContentStorageOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly Repository _repository;

        private readonly AsyncLock _asyncLock = new AsyncLock();

        const int MaxBlockLength = 1 * 1024 * 1024;

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

            _repository = new Repository(Path.Combine(_options.ConfigPath, "lite.db"));
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
            using (await _asyncLock.LockAsync())
            {
                throw new NotImplementedException();
            }
        }

        public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
        }

        public async ValueTask<IEnumerable<OmniHash>> GetContentHashesAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var results = new List<OmniHash>();

                foreach (var status in _repository.PushStatus.GetAll())
                {
                    results.Add(status.Hash);
                }

                return results;
            }
        }

        public async ValueTask<IEnumerable<OmniHash>> GetBlockHashesAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _repository.PushStatus.Get(rootHash);
                if (status == null) return Enumerable.Empty<OmniHash>();

                return status.MerkleTreeSections.SelectMany(n => n.Hashes);
            }
        }

        public async ValueTask<bool> ContainsContentAsync(OmniHash rootHash)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _repository.PushStatus.Get(rootHash);
                if (status == null) return false;

                return true;
            }
        }

        public async ValueTask<bool> ContainsBlockAsync(OmniHash rootHash, OmniHash targetHash)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _repository.PushStatus.Get(rootHash);
                if (status == null) return false;

                return status.MerkleTreeSections.Any(n => n.Contains(rootHash));
            }
        }

        public async ValueTask<OmniHash> RegisterPushContentAsync(string filePath, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                // 既にエンコード済みの場合
                {
                    var status = _repository.PushStatus.Get(filePath);
                    if (status != null) return status.Hash;
                }

                // エンコード処理
                {
                    var tempPath = Path.Combine(_options.ConfigPath, "cache", "_temp_");

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

                                    await this.WriteBlockAsync(tempPath, hash, memory);

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

                                await this.WriteBlockAsync(tempPath, hash, memory);

                                rootHash = hash;
                            }

                            break;
                        }
                    }

                    // 一時フォルダからキャッシュフォルダへ移動させる
                    {
                        var cachePath = Path.Combine(_options.ConfigPath, "cache", HashToString(rootHash));
                        Directory.Move(tempPath, cachePath);
                    }

                    var status = new PushStatus(rootHash, filePath, merkleTreeSections.ToArray());
                    _repository.PushStatus.Add(status);

                    return rootHash;
                }
            }
        }

        private async ValueTask WriteBlockAsync(string basePath, OmniHash hash, ReadOnlyMemory<byte> memory)
        {
            var filePath = Path.Combine(basePath, HashToString(hash));

            using (var fileStream = new UnbufferedFileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bytesPool))
            {
                await fileStream.WriteAsync(memory);
            }
        }

        public async ValueTask UnregisterPushContentAsync(string filePath, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _repository.PushStatus.Get(filePath);
                if (status == null) return;
                _repository.PushStatus.Remove(filePath);

                // キャッシュフォルダを削除
                var cacheDirPath = Path.Combine(_options.ConfigPath, HashToString(status.Hash));

                Directory.Delete(cacheDirPath);
            }
        }

        public async ValueTask<IMemoryOwner<byte>?> ReadBlockAsync(OmniHash rootHash, OmniHash targetHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var filePath = Path.Combine(Path.Combine(_options.ConfigPath, "cache", HashToString(rootHash)), HashToString(targetHash));
                if (!File.Exists(filePath)) return null;

                using (var fileStream = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None, _bytesPool))
                {
                    var memoryOwner = _bytesPool.Memory.Rent((int)fileStream.Length);
                    await fileStream.ReadAsync(memoryOwner.Memory);

                    return memoryOwner;
                }
            }
        }

        private static string HashToString(OmniHash hash)
        {
            return hash.ToString(ConvertStringType.Base16);
        }

        private class PushStatus
        {
            public PushStatus(OmniHash hash, string filePath, MerkleTreeSection[] merkleTreeSections)
            {
                this.FilePath = filePath;
                this.Hash = hash;
                this.MerkleTreeSections = new ReadOnlyListSlim<MerkleTreeSection>(merkleTreeSections);
            }

            public string FilePath { get; }
            public OmniHash Hash { get; }
            public ReadOnlyListSlim<MerkleTreeSection> MerkleTreeSections { get; }
        }

        private sealed class Repository : IDisposable
        {
            private readonly LiteDatabase _database;

            public Repository(string path)
            {
                _database = new LiteDatabase(path);
                this.PushStatus = new PushStatusRepository(_database);
            }

            public void Dispose()
            {
                _database.Dispose();
            }

            public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
            {
                if (0 <= _database.UserVersion)
                {
                    var wants = _database.GetCollection<PushStatusEntity>("pushes");
                    wants.EnsureIndex(x => x.Hash, false);
                    wants.EnsureIndex(x => x.FilePath, true);
                    _database.UserVersion = 1;
                }
            }

            public PushStatusRepository PushStatus { get; set; }

            public sealed class PushStatusRepository
            {
                private readonly LiteDatabase _database;

                public PushStatusRepository(LiteDatabase database)
                {
                    _database = database;
                }

                public IEnumerable<PushStatus> GetAll()
                {
                    var col = _database.GetCollection<PushStatusEntity>("pushes");
                    return col.FindAll().Select(n => n.Export());
                }

                public PushStatus? Get(OmniHash rootHash)
                {
                    var col = _database.GetCollection<PushStatusEntity>("pushes");
                    var param = OmniHashEntity.Import(rootHash);
                    return col.FindOne(n => n.Hash == param)?.Export();
                }

                public PushStatus? Get(string filePath)
                {
                    var col = _database.GetCollection<PushStatusEntity>("pushes");
                    return col.FindOne(n => n.FilePath == filePath)?.Export();
                }

                public void Add(PushStatus status)
                {
                    var col = _database.GetCollection<PushStatusEntity>("pushes");
                    var param = PushStatusEntity.Import(status);
                    col.Insert(param);
                }

                public void Remove(OmniHash rootHash)
                {
                    var col = _database.GetCollection<PushStatusEntity>("pushes");
                    var param = OmniHashEntity.Import(rootHash);
                    col.DeleteMany(n => n.Hash == param);
                }

                public void Remove(string filePath)
                {
                    var col = _database.GetCollection<PushStatusEntity>("pushes");
                    col.DeleteMany(n => n.FilePath == filePath);
                }
            }

            private class PushStatusEntity
            {
                public string? FilePath { get; set; }
                public OmniHashEntity? Hash { get; set; }
                public MerkleTreeSectionEntity[]? MerkleTreeSections { get; set; }

                public static PushStatusEntity Import(PushStatus value)
                {
                    return new PushStatusEntity()
                    {
                        Hash = OmniHashEntity.Import(value.Hash),
                        FilePath = value.FilePath,
                        MerkleTreeSections = value.MerkleTreeSections.Select(n => MerkleTreeSectionEntity.Import(n)).ToArray()
                    };
                }

                public PushStatus Export()
                {
                    return new PushStatus(this.Hash?.Export() ?? OmniHash.Empty, this.FilePath ?? string.Empty, this.MerkleTreeSections.Select(n => n.Export()).ToArray());
                }
            }
        }
    }
}
