using System.Buffers;
using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Primitives;

public interface IReadOnlyFileStorage
{
    ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default);
    ValueTask<IMemoryOwner<byte>?> TryReadBlockAsync(OmniHash rootHash, OmniHash blockHash, CancellationToken cancellationToken = default);
}
