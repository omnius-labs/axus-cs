using System;
using System.Collections.Generic;
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

        ValueTask<OmniHash> AddPublishFile(string filePath, CancellationToken cancellationToken = default);
        void RemovePublishFile(string filePath);
        IEnumerable<PublishFileReport> GetPublishFileReports();

        void AddWantFile(OmniHash rootHash, string filePath);
        void RemoveWantFile(OmniHash rootHash, string filePath);
        IEnumerable<WantFileReport> GetWantFileReports();
    }
}
