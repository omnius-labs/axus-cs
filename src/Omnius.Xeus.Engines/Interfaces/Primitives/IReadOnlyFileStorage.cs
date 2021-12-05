using System.Buffers;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines.Primitives;

public interface IReadOnlyFileStorage
{
    ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default);

    ValueTask<IMemoryOwner<byte>?> ReadBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default);
}
