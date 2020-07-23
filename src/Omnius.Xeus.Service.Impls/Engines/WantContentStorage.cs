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

namespace Omnius.Xeus.Service.Engines
{
    public sealed class WantContentStorage : AsyncDisposableBase, IWantContentStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly WantContentStorageOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly LiteDatabase _database;

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

        internal WantContentStorage(WantContentStorageOptions options, IBytesPool bytesPool)
        {
            _options = options;
            _bytesPool = bytesPool;

            _database = new LiteDatabase(Path.Combine(_options.ConfigPath, "lite.db"));
        }

        internal async ValueTask InitAsync()
        {
            await this.MigrateAsync();
        }

        private async ValueTask MigrateAsync(CancellationToken cancellationToken = default)
        {
            if (0 <= _database.UserVersion)
            {
                var wants = _database.GetCollection<WantEntity>("wants");
                wants.EnsureIndex(x => x.Hash, true);
                _database.UserVersion = 1;
            }
        }

        protected override async ValueTask OnDisposeAsync()
        {

        }

        public ValueTask<WantContentStorageReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
        }

        public async ValueTask<bool> ContainsAsync(OmniHash rootHash)
        {
            using (await _asyncLock.LockAsync())
            {
                var rootHashString = rootHash.ToString(ConvertStringType.Base16);

                var wants = _database.GetCollection<WantEntity>("wants");
                if (!wants.Exists(n => n.Hash == rootHashString)) return false;
                return true;
            }
        }

        public ValueTask<bool> ContainsAsync(OmniHash rootHash, OmniHash targetHash)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<IMemoryOwner<byte>?> ReadAsync(OmniHash rootHash, OmniHash targetHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var rootHashString = rootHash.ToString(ConvertStringType.Base16);
                var targetHashString = rootHash.ToString(ConvertStringType.Base16);

                var wants = _database.GetCollection<WantEntity>("wants");
                if (!wants.Exists(n => n.Hash == rootHashString)) return null;

                var filePath = Path.Combine(Path.Combine(_options.ConfigPath, rootHash.ToString(ConvertStringType.Base16), targetHash.ToString(ConvertStringType.Base16)));

                if (!File.Exists(filePath)) return null;

                using (var fileStream = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None, _bytesPool))
                {
                    var memoryOwner = _bytesPool.Memory.Rent((int)fileStream.Length);
                    await fileStream.ReadAsync(memoryOwner.Memory);

                    return memoryOwner;
                }
            }
        }

        public async ValueTask WriteAsync(OmniHash rootHash, OmniHash targetHash, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var rootHashString = rootHash.ToString(ConvertStringType.Base16);
                var targetHashString = rootHash.ToString(ConvertStringType.Base16);

                var wants = _database.GetCollection<WantEntity>("wants");
                if (!wants.Exists(n => n.Hash == rootHashString)) return;

                var filePath = Path.Combine(Path.Combine(_options.ConfigPath, rootHash.ToString(ConvertStringType.Base16), targetHash.ToString(ConvertStringType.Base16)));
                var directoryPath = Path.GetDirectoryName(filePath);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (var fileStream = new UnbufferedFileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bytesPool))
                {
                    await fileStream.WriteAsync(memory);
                }
            }
        }

        public ValueTask ExportAsync(OmniHash rootHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask ExportAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<IEnumerable<ResourceTag>> GetWantTagsAsync()
        {
            using (await _asyncLock.LockAsync())
            {
                var results = new List<ResourceTag>();

                var wants = _database.GetCollection<WantEntity>("wants");

                foreach (var want in wants.FindAll())
                {
                    if (want?.Hash == null || !OmniHash.TryParse(want.Hash, out var hash)) continue;
                    results.Add(new ResourceTag("declared_message", hash));
                }

                return results;
            }
        }

        public async ValueTask WantAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var rootHashString = rootHash.ToString(ConvertStringType.Base16);

                var wants = _database.GetCollection<WantEntity>("wants");
                wants.Insert(new WantEntity() { Hash = rootHashString });
            }
        }

        public async ValueTask UnwantAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var rootHashString = rootHash.ToString(ConvertStringType.Base16);

                var wants = _database.GetCollection<WantEntity>("wants");
                wants.DeleteMany(n => n.Hash == rootHashString);
            }
        }

        private sealed class WantEntity
        {
            public int Id { get; set; }
            public string? Hash { get; set; }
        }
    }
}
