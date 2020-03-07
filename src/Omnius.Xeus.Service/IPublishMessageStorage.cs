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
        ValueTask<IPublishMessageStorage> CreateAsync(string configPath, IBytesPool bytesPool);
    }

    public interface IPublishMessageStorage : IPublishStorage
    {
        ValueTask<OmniHash> AddPublishMessageAsync(ReadOnlySequence<byte> sequence, CancellationToken cancellationToken = default);
        ValueTask RemovePublishMessageAsync(OmniHash rootHash, CancellationToken cancellationToken = default);
    }
}
