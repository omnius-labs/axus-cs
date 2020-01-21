using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public interface IWantFileStorageFactory
    {
        ValueTask<IWantFileStorage> Create(string configPath, IBufferPool<byte> bufferPool);
    }

    public interface IWantFileStorage : IWritableStorage, IAsyncDisposable
    {
        public static IWantFileStorageFactory Factory { get; }

        ValueTask AddAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
        ValueTask RemoveAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
        ValueTask<bool> TryExportAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default);
        IAsyncEnumerable<WantFileReport> GetReportsAsync(CancellationToken cancellationToken = default);
    }
}
