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
    public interface IFileStorageFactory
    {
        public ValueTask<IFileStorage> Create(string configPath, IBufferPool<byte> bufferPool);
    }

    public interface IFileStorage : IPrimitiveStorage, IAsyncDisposable
    {
        public static IFileStorageFactory Factory { get; }

        ValueTask<OmniHash> AddPublishFileAsync(string filePath, CancellationToken cancellationToken = default);
        ValueTask RemovePublishFileAsync(string filePath, CancellationToken cancellationToken = default);
        IAsyncEnumerable<PublishFileReport> GetPublishFileReportsAsync(CancellationToken cancellationToken = default);

        ValueTask AddWantFileAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default);
        ValueTask RemoveWantFileAsync(OmniHash rootHash, string filePath, CancellationToken cancellationToken = default);
        IAsyncEnumerable<WantFileReport> GetWantFileReportsAsync(CancellationToken cancellationToken = default);
    }
}
