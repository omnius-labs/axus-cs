using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Io;
using Omnius.Core.Serialization;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages.Internal.Models;
using Omnius.Xeus.Engines.Storages.Internal.Repositories;

namespace Omnius.Xeus.Engines.Storages
{
    public sealed partial class WantContentStorage : AsyncDisposableBase, IWantContentStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly WantContentStorageOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly WantContentStorageRepository _database;

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

            _database = new WantContentStorageRepository(Path.Combine(_options.ConfigPath, "database"));
        }

        internal async ValueTask InitAsync()
        {
            await _database.MigrateAsync();
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _database.Dispose();
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
                var status = _database.WantStatus.Get(rootHash);
                if (status == null) return Enumerable.Empty<OmniHash>();

                throw new NotImplementedException();
            }
        }

        public async ValueTask<bool> ContainsContentAsync(OmniHash rootHash)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _database.WantStatus.Get(rootHash);
                if (status == null) return false;

                return true;
            }
        }

        public async ValueTask<bool> ContainsBlockAsync(OmniHash rootHash, OmniHash targetHash)
        {
            using (await _asyncLock.LockAsync())
            {
                var status = _database.WantStatus.Get(rootHash);
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
                _database.WantStatus.Add(new WantContentStatus(rootHash));
            }
        }

        public async ValueTask UnregisterWantContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                _database.WantStatus.Remove(rootHash);
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
                var status = _database.WantStatus.Get(rootHash);
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
                var status = _database.WantStatus.Get(rootHash);
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
    }
}
