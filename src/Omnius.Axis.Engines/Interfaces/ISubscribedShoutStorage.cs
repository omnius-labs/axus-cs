using Omnius.Axis.Engines.Primitives;
using Omnius.Axis.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axis.Engines;

public interface ISubscribedShoutStorage : IWritableShoutStorage, IAsyncDisposable
{
    ValueTask<IEnumerable<SubscribedShoutReport>> GetSubscribedShoutReportsAsync(CancellationToken cancellationToken = default);

    ValueTask SubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default);

    ValueTask UnsubscribeShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default);
}
