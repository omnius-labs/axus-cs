using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public interface IWantMessageStorageFactory
    {
        ValueTask<IWantMessageStorage> CreateAsync(string configPath, IBufferPool<byte> bufferPool);
    }

    public interface IWantMessageStorage : IWritableStorage, IAsyncDisposable
    {
        public static IWantMessageStorageFactory Factory { get; }

        ValueTask AddAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
        ValueTask RemoveAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
        ValueTask<bool> TryExportAsync(OmniHash rootHash, IBufferWriter<byte> bufferWriter, CancellationToken cancellationToken = default);
        IAsyncEnumerable<WantMessageReport> GetReportsAsync(CancellationToken cancellationToken = default);
    }
}
