using Omnius.Axus.Engines.Primitives;
using Omnius.Axus.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines;

public interface ISubscribedShoutStorage : IWritableShoutStorage, IAsyncDisposable
{
    ValueTask<IEnumerable<SubscribedShoutReport>> GetSubscribedShoutReportsAsync(CancellationToken cancellationToken = default);
    ValueTask SubscribeShoutAsync(OmniSignature signature, string channel, string author, CancellationToken cancellationToken = default);
    ValueTask UnsubscribeShoutAsync(OmniSignature signature, string channel, string author, CancellationToken cancellationToken = default);
}
