using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public interface IPublishMessageStorageFactory
    {
        ValueTask<IPublishMessageStorage> CreateAsync(string configPath, IBufferPool<byte> bufferPool);
    }

    public interface IPublishMessageStorage : IReadOnlyStorage
    {
        public static IPublishMessageStorageFactory Factory { get; }

        ValueTask<OmniHash> AddAsync(ReadOnlySequence<byte> sequence, CancellationToken cancellationToken = default);
        ValueTask RemoveAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
        IEnumerable<PublishMessageReport> GetReportsAsync(CancellationToken cancellationToken = default);
    }
}
