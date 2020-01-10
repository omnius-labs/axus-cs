using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
        private readonly string _configPath;
        private readonly IBufferPool<byte> _bufferPool;

        private readonly AsyncLock _asyncLock = new AsyncLock();

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
        }

        public async ValueTask InitAsync()
        {
        }

        protected override async ValueTask OnDisposeAsync()
        {

        }

        public bool TryRead(OmniHash rootHash, OmniHash targetHash, [NotNullWhen(true)] out IMemoryOwner<byte>? memoryOwner)
        {
            throw new NotImplementedException();
        }

        public bool TryWrite(OmniHash rootHash, OmniHash targetHash, ReadOnlySpan<byte> value)
        {
            throw new NotImplementedException();
        }

        public ulong TotalUsingBytes { get; }

        public ValueTask CheckConsistency(Action<CheckConsistencyReport> callback, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public bool Contains(OmniHash rootHash, OmniHash targetHash)
        {
            throw new NotImplementedException();
        }

        public uint GetLength(OmniHash rootHash, OmniHash targetHash)
        {
            throw new NotImplementedException();
        }
    }
}
