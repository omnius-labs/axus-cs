using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Primitives;

public interface IWritableFileStorage : IReadOnlyFileStorage
{
    ValueTask WriteBlockAsync(OmniHash rootHash, OmniHash blockHash, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default);
}
