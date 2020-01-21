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

namespace Omnius.Xeus.Service
{
    public sealed class WantFileStorage : AsyncDisposableBase, IWantFileStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _configPath;
        private readonly IBufferPool<byte> _bufferPool;

        private readonly Dictionary<OmniHash, WantFileStatus> _wantFileStatusMap = new Dictionary<OmniHash, WantFileStatus>();

        private readonly AsyncLock _asyncLock = new AsyncLock();

        const int MaxBlockLength = 1 * 1024 * 1024;

        internal sealed class WantFileStorageFactory : IWantFileStorageFactory
        {
            public async ValueTask<IWantFileStorage> Create(string configPath, IBufferPool<byte> bufferPool)
            {
                var result = new WantFileStorage(configPath, bufferPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IWantFileStorageFactory Factory { get; } = new WantFileStorageFactory();

        internal WantFileStorage(string configPath, IBufferPool<byte> bufferPool)
        {
            _configPath = configPath;
            _bufferPool = bufferPool;
        }

        public async ValueTask InitAsync()
        {
            await this.StartWatchWantBlocks();
        }

        protected override async ValueTask OnDisposeAsync()
        {
            await this.StopWatchWantBlocks();
        }

        public async ValueTask CheckConsistencyAsync(Action<CheckConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
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
                if (!_wantFileStatusMap.ContainsKey(rootHash))
                {
                    return null;
                }

                var filePath = Path.Combine(Path.Combine(_configPath, this.OmniHashToString(rootHash)), this.OmniHashToString(targetHash));

                if (!File.Exists(filePath))
                {
                    return null;
                }

                using (var fileStream = new UnbufferedFileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.None, _bufferPool))
                {
                    var memoryOwner = _bufferPool.Memory.Rent((int)fileStream.Length);
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

                var filePath = Path.Combine(Path.Combine(_configPath, this.OmniHashToString(rootHash)), this.OmniHashToString(targetHash));

                if (File.Exists(filePath))
                {
                    return;
                }

                using (var fileStream = new UnbufferedFileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None, _bufferPool))
                {
                    await fileStream.WriteAsync(memory);
                }

                status.WantBlocks.Remove(targetHash);
            }
        }

        public async ValueTask AddAsync(OmniHash rootHash,  CancellationToken cancellationToken = default)
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

        public async ValueTask RemoveAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                _wantFileStatusMap.Remove(rootHash);
            }
        }

        public ValueTask<bool> TryExportAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async IAsyncEnumerable<WantFileReport> GetReportsAsync([EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                foreach (var status in _wantFileStatusMap.Values)
                {
                    yield return new WantFileReport(status.RootHash, status.WantBlocks.ToArray());
                }
            }
        }

        private async ValueTask StartWatchWantBlocks()
        {

        }

        private async ValueTask StopWatchWantBlocks()
        {

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
