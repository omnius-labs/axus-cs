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
    public interface IPublishFileStorageFactory
    {
        ValueTask<IPublishFileStorage> Create(string configPath, IBufferPool<byte> bufferPool);
    }

    public interface IPublishFileStorage : IReadOnlyStorage, IAsyncDisposable
    {
        public static IPublishFileStorageFactory Factory { get; }

        ValueTask<OmniHash> AddAsync(string filePath, CancellationToken cancellationToken = default);
        ValueTask RemoveAsync(string filePath, CancellationToken cancellationToken = default);
        IAsyncEnumerable<PublishFileReport> GetReportsAsync(CancellationToken cancellationToken = default);
    }
}
