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
using Omnius.Xeus.Service.Models;
using Omnius.Xeus.Service.Storages.Internal;

namespace Omnius.Xeus.Service.Storages
{
    public sealed class WantContentStorage : AsyncDisposableBase, IWantContentStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly WantContentStorageOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly Repository _repository;

        private readonly AsyncLock _asyncLock = new AsyncLock();

        const int MaxBlockLength = 1 * 1024 * 1024;

        internal sealed class WantContentStorageFactory : IWantContentStorageFactory
        {
            public async ValueTask<IWantContentStorage> CreateAsync(WantContentStorageOptions options, IBytesPool bytesPool)
            {
                var result = new WantContentStorage(options, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IWantContentStorageFactory Factory { get; } = new WantContentStorageFactory();

        private WantContentStorage(WantContentStorageOptions options, IBytesPool bytesPool)
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

        public ValueTask<WantContentStorageReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
        }

        public async ValueTask<IEnumerable<OmniHash>> GetContentHashesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<IEnumerable<OmniHash>> GetBlockHashesAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _repository.WantStatus.Get(rootHash);
                if (status == null) return Enumerable.Empty<OmniHash>();

                throw new NotImplementedException();
            }
        }

        public async ValueTask<bool> ContainsContentAsync(OmniHash rootHash)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _repository.WantStatus.Get(rootHash);
                if (status == null) return false;

                return true;
            }
        }

        public async ValueTask<bool> ContainsBlockAsync(OmniHash rootHash, OmniHash targetHash)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _repository.WantStatus.Get(rootHash);
                if (status == null) return false;

                var filePath = Path.Combine(Path.Combine(_options.ConfigPath, "cache", HashToString(rootHash), HashToString(targetHash)));
                if (!File.Exists(filePath)) return false;

                return true;
            }
        }

        public async ValueTask RegisterWantContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                _repository.WantStatus.Add(new WantStatus(rootHash));
            }
        }

        public async ValueTask UnregisterWantContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                _repository.WantStatus.Remove(rootHash);
            }
        }

        public ValueTask ExportContentAsync(OmniHash rootHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask ExportContentAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<IMemoryOwner<byte>?> ReadBlockAsync(OmniHash rootHash, OmniHash targetHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _repository.WantStatus.Get(rootHash);
                if (status == null) return null;

                var filePath = Path.Combine(Path.Combine(_options.ConfigPath, "cache", HashToString(rootHash), HashToString(targetHash)));
                if (!File.Exists(filePath)) return null;

                using var fileStream = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None, _bytesPool);
                var memoryOwner = _bytesPool.Memory.Rent((int)fileStream.Length);
                await fileStream.ReadAsync(memoryOwner.Memory);
                return memoryOwner;
            }
        }

        public async ValueTask WriteBlockAsync(OmniHash rootHash, OmniHash targetHash, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _repository.WantStatus.Get(rootHash);
                if (status == null) return;

                var filePath = Path.Combine(Path.Combine(_options.ConfigPath, "cache", HashToString(rootHash), HashToString(targetHash)));
                var directoryPath = Path.GetDirectoryName(filePath);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using var fileStream = new UnbufferedFileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bytesPool);
                await fileStream.WriteAsync(memory);
            }
        }

        private static string HashToString(OmniHash hash)
        {
            return hash.ToString(ConvertStringType.Base16);
        }

        private sealed class WantStatus
        {
            public WantStatus(OmniHash hash)
            {
                this.Hash = hash;
            }

            public OmniHash Hash { get; }
        }

        private sealed class Repository : IDisposable
        {
            private readonly LiteDatabase _database;

            public Repository(string path)
            {
                _database = new LiteDatabase(path);
                this.WantStatus = new WantStatusRepository(_database);
            }

            public void Dispose()
            {
                _database.Dispose();
            }

            public async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
            {
                if (0 <= _database.UserVersion)
                {
                    var wants = _database.GetCollection<WantStatusEntity>("wants");
                    wants.EnsureIndex(x => x.Hash, true);
                    _database.UserVersion = 1;
                }
            }

            public WantStatusRepository WantStatus { get; set; }

            public sealed class WantStatusRepository
            {
                private readonly LiteDatabase _database;

                public WantStatusRepository(LiteDatabase database)
                {
                    _database = database;
                }

                public IEnumerable<WantStatus> GetAll()
                {
                    var col = _database.GetCollection<WantStatusEntity>("wants");
                    return col.FindAll().Select(n => n.Export());
                }

                public WantStatus? Get(OmniHash rootHash)
                {
                    var col = _database.GetCollection<WantStatusEntity>("wants");
                    var param = OmniHashEntity.Import(rootHash);
                    return col.FindOne(n => n.Hash == param)?.Export();
                }

                public void Add(WantStatus status)
                {
                    var col = _database.GetCollection<WantStatusEntity>("wants");
                    var param = WantStatusEntity.Import(status);
                    col.Insert(param);
                }

                public void Remove(OmniHash rootHash)
                {
                    var col = _database.GetCollection<WantStatusEntity>("wants");
                    var param = OmniHashEntity.Import(rootHash);
                    col.DeleteMany(n => n.Hash == param);
                }
            }

            private sealed class WantStatusEntity
            {
                public int Id { get; set; }
                public OmniHashEntity? Hash { get; set; }

                public static WantStatusEntity Import(WantStatus value)
                {
                    return new WantStatusEntity() { Hash = OmniHashEntity.Import(value.Hash) };
                }

                public WantStatus Export()
                {
                    return new WantStatus(this.Hash?.Export() ?? OmniHash.Empty);
                }
            }
        }
    }
}
