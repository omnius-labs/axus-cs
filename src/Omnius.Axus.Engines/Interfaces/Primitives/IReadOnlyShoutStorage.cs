using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Primitives;

public interface IReadOnlyShoutStorage
{
    ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<(OmniSignature, string)>> GetKeysAsync(CancellationToken cancellationToken = default);
    ValueTask<bool> ContainsShoutAsync(OmniSignature signature, string channel, CancellationToken cancellationToken = default);
    ValueTask<DateTime> ReadShoutUpdatedTimeAsync(OmniSignature signature, string channel, CancellationToken cancellationToken = default);
    ValueTask<Shout?> TryReadShoutAsync(OmniSignature signature, string channel, DateTime updatedTime, CancellationToken cancellationToken = default);
}
