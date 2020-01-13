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
        public ValueTask<IWantFileStorage> Create(string configPath, IBufferPool<byte> bufferPool);
    }

    public interface IWantFileStorage : IStorage, IAsyncDisposable
    {
        public static IWantFileStorageFactory Factory { get; }

        ValueTask AddWantFileAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
        ValueTask RemoveWantFileAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
        ValueTask ExportAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
        IAsyncEnumerable<WantFileReport> GetWantFileReportsAsync(CancellationToken cancellationToken = default);
    }
}
