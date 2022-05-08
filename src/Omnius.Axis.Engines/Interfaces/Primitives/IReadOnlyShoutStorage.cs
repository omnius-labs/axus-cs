using Omnius.Axis.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axis.Engines.Primitives;

public interface IReadOnlyShoutStorage
{
    ValueTask CheckConsistencyAsync(Action<ConsistencyReport> callback, CancellationToken cancellationToken = default);

    ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default);

    ValueTask<bool> ContainsShoutAsync(OmniSignature signature, CancellationToken cancellationToken = default);

    ValueTask<DateTime?> ReadShoutCreatedTimeAsync(OmniSignature signature, CancellationToken cancellationToken = default);

    ValueTask<Shout?> ReadShoutAsync(OmniSignature signature, CancellationToken cancellationToken = default);
}
