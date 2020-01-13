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
        public ValueTask<IPublishFileStorage> Create(string configPath, IBufferPool<byte> bufferPool);
    }

    public interface IPublishFileStorage : IReadOnlyStorage, IAsyncDisposable
    {
        public static IPublishFileStorageFactory Factory { get; }

        ValueTask<OmniHash> AddPublishFileAsync(string filePath, CancellationToken cancellationToken = default);
        ValueTask RemovePublishFileAsync(string filePath, CancellationToken cancellationToken = default);
        IAsyncEnumerable<PublishFileReport> GetPublishFileReportsAsync(CancellationToken cancellationToken = default);
    }
}
