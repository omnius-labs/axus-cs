using Omnius.Axus.Engines.Primitives;
using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines;

public interface IShoutSubscriberStorage : IWritableShoutStorage, IAsyncDisposable
{
    ValueTask<IEnumerable<SubscribedShoutReport>> GetSubscribedShoutReportsAsync(CancellationToken cancellationToken = default);
    ValueTask SubscribeShoutAsync(OmniSignature signature, AttachedProperty? property, string channel, CancellationToken cancellationToken = default);
    ValueTask UnsubscribeShoutAsync(OmniSignature signature, string channel, CancellationToken cancellationToken = default);
}
