using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Service.Engines.Primitives;

public interface IWritableFileStorage : IReadOnlyFileStorage
{
    ValueTask WriteBlockAsync(OmniHash rootHash, OmniHash blockHash, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default);
}
