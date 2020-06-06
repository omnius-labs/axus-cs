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

namespace Omnius.Xeus.Service.Engines
{
    public sealed class WantStorage : AsyncDisposableBase, IWantStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _configPath;
        private readonly IBytesPool _bytesPool;

        private readonly Dictionary<OmniHash, WantFileStatus> _wantFileStatusMap = new Dictionary<OmniHash, WantFileStatus>();

        private readonly AsyncLock _asyncLock = new AsyncLock();

        const int MaxBlockLength = 1 * 1024 * 1024;

        internal sealed class WantStorageFactory : IWantStorageFactory
        {
            public async ValueTask<IWantStorage> CreateAsync(string configPath, WantStorageOptions options, IObjectStoreFactory objectStoreFactory, IBytesPool bytesPool)
            {
                var result = new WantStorage(configPath, options, objectStoreFactory, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IWantStorageFactory Factory { get; } = new WantStorageFactory();

        internal WantStorage(string configPath, WantStorageOptions options, IObjectStoreFactory objectStoreFactory, IBytesPool bytesPool)
        {
            _configPath = configPath;
            _bytesPool = bytesPool;
        }

        internal async ValueTask InitAsync()
        {
            await this.StartWatchWantBlocks();
        }

        protected override async ValueTask OnDisposeAsync()
        {
            await this.StopWatchWantBlocks();
        }

        private async ValueTask StartWatchWantBlocks()
        {

        }

        private async ValueTask StopWatchWantBlocks()
        {

        }

        public async ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default)
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
                if (!_wantFileStatusMap.ContainsKey(rootHash))
                {
                    return null;
                }

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

        public async ValueTask WriteAsync(OmniHash rootHash, OmniHash targetHash, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                if (!_wantFileStatusMap.TryGetValue(rootHash, out var status)
                    || !status.WantBlocks.Contains(targetHash))
                {
                    return;
                }

                var filePath = Path.Combine(Path.Combine(_configPath, this.OmniHashToFilePath(rootHash)), this.OmniHashToFilePath(targetHash));

                if (File.Exists(filePath))
                {
                    return;
                }

                using (var fileStream = new UnbufferedFileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bytesPool))
                {
                    await fileStream.WriteAsync(memory);
                }

                status.WantBlocks.Remove(targetHash);
            }
        }

        public async ValueTask AddWantFileAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                if (_wantFileStatusMap.ContainsKey(rootHash))
                {
                    return;
                }

                var status = new WantFileStatus(rootHash);
                status.CurrentDepth = 0;
                status.WantBlocks.Add(rootHash);

                _wantFileStatusMap.Add(rootHash, status);
            }
        }

        public async ValueTask RemoveWantFileAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                _wantFileStatusMap.Remove(rootHash);
            }
        }

        public ValueTask<bool> TryExportWantFileAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<WantReport[]> GetReportsAsync([EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var result = new List<WantReport>();

                foreach (var status in _wantFileStatusMap.Values)
                {
                    result.Add(new WantReport(status.RootHash, status.WantBlocks.ToArray()));
                }

                return result.ToArray();
            }
        }

        public ValueTask WantAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask UnwantAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask ExportAsync(OmniHash rootHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask ExportAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        class WantFileStatus
        {
            public WantFileStatus(OmniHash rootHash)
            {
                this.RootHash = rootHash;
            }

            public OmniHash RootHash { get; }

            public int CurrentDepth { get; set; }
            public HashSet<OmniHash> WantBlocks { get; } = new HashSet<OmniHash>();
        }
    }
}
