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

namespace Omnius.Xeus.Service.Engines
{
    public sealed class WantContentStorage : AsyncDisposableBase, IWantContentStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly WantContentStorageOptions _options;
        private readonly IBytesPool _bytesPool;

        private readonly Dictionary<OmniHash, WantFileStatus> _wantFileStatusMap = new Dictionary<OmniHash, WantFileStatus>();

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

        public bool Contains(OmniHash rootHash)
        {
            throw new NotImplementedException();
        }

        public bool Contains(OmniHash rootHash, OmniHash targetHash)
        {
            throw new NotImplementedException();
        }

        private string GenFilePath(OmniHash hash)
        {
            return hash.ToString(ConvertStringType.Base16, ConvertStringCase.Lower);
        }

        public async ValueTask<IMemoryOwner<byte>?> ReadAsync(OmniHash rootHash, OmniHash targetHash, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                if (!_wantFileStatusMap.ContainsKey(rootHash))
                {
                    return null;
                }

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

        public async ValueTask WriteAsync(OmniHash rootHash, OmniHash targetHash, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                if (!_wantFileStatusMap.TryGetValue(rootHash, out var status) || !status.WantBlocks.Contains(targetHash))
                {
                    return;
                }

                var filePath = Path.Combine(Path.Combine(_options.ConfigPath, this.GenFilePath(rootHash)), this.GenFilePath(targetHash));

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

        public ValueTask<WantContentStorageReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask WantContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask UnwantContentAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask ExportContentAsync(OmniHash rootHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask ExportContentAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ResourceTag> GetWantTags()
        {
            throw new NotImplementedException();
        }

        public bool ContainsWantTag(ResourceTag tag)
        {
            throw new NotImplementedException();
        }

        IEnumerable<ResourceTag> IWantStorage.GetWantTags()
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
