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
using Omnius.Core.Serialization;
using Omnius.Xeus.Service.Internal;

namespace Omnius.Xeus.Service
{
    public sealed partial class FileStorage : AsyncDisposableBase, IFileStorage
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _configPath;
        private readonly IBufferPool<byte> _bufferPool;

        private readonly WantFileStorage _wantFileStorage;
        private readonly PublishFileStorage _publishFileStorage;

        const int MaxBlockLength = 1 * 1024 * 1024;

        internal sealed class FileStorageFactory : IFileStorageFactory
        {
            public async ValueTask<IFileStorage> Create(string configPath, IBufferPool<byte> bufferPool)
            {
                var result = new FileStorage(configPath, bufferPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IFileStorageFactory Factory { get; } = new FileStorageFactory();

        internal FileStorage(string configPath, IBufferPool<byte> bufferPool)
        {
            _configPath = configPath;
            _bufferPool = bufferPool;

            _wantFileStorage = new WantFileStorage(_configPath, _bufferPool);
            _publishFileStorage = new PublishFileStorage(_configPath, _bufferPool);
        }

        public async ValueTask InitAsync()
        {
        }

        protected override async ValueTask OnDisposeAsync()
        {
        }

        public async ValueTask CheckConsistencyAsync(Action<CheckConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<IMemoryOwner<byte>?> ReadAsync(OmniHash rootHash, OmniHash targetHash, CancellationToken cancellationToken = default)
        {
            // WantFile
            {
                var memoryOwner = await _wantFileStorage.ReadAsync(rootHash, targetHash, cancellationToken);

                if (memoryOwner != null)
                {
                    return memoryOwner;
                }
            }

            // PublishFile
            {
                var memoryOwner = await _publishFileStorage.ReadAsync(rootHash, targetHash, cancellationToken);

                if (memoryOwner != null)
                {
                    return memoryOwner;
                }
            }

            return null;
        }

        public ValueTask WriteAsync(OmniHash rootHash, OmniHash targetHash, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
        {
            return _wantFileStorage.WriteAsync(rootHash, targetHash, memory, cancellationToken);
        }

        public ValueTask AddWantFileAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default)
        {
            return _wantFileStorage.AddWantFileAsync(rootHash, filePath, cancellationToken);
        }

        public ValueTask RemoveWantFileAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default)
        {
            return _wantFileStorage.RemoveWantFile(rootHash, filePath, cancellationToken);
        }

        public IAsyncEnumerable<WantFileReport> GetWantFileReportsAsync(CancellationToken cancellationToken = default)
        {
            return _wantFileStorage.GetWantFileReportsAsync(cancellationToken);
        }

        public ValueTask<OmniHash> AddPublishFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return _publishFileStorage.AddPublishFileAsync(filePath, cancellationToken);
        }

        public ValueTask RemovePublishFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return _publishFileStorage.RemovePublishFileAsync(filePath, cancellationToken);
        }

        public IAsyncEnumerable<PublishFileReport> GetPublishFileReportsAsync(CancellationToken cancellationToken = default)
        {
            return _publishFileStorage.GetPublishFileReportsAsync();
        }
    }
}
