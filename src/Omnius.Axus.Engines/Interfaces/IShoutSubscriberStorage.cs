using Omnius.Axus.Engines.Primitives;
using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines;

public interface IShoutSubscriberStorage : IWritableShoutStorage, IAsyncDisposable
{
    ValueTask<IEnumerable<SubscribedShoutReport>> GetSubscribedShoutReportsAsync(string zone, CancellationToken cancellationToken = default);
    ValueTask SubscribeShoutAsync(OmniSignature signature, IEnumerable<AttachedProperty> properties, string channel, string zone, CancellationToken cancellationToken = default);
    ValueTask UnsubscribeShoutAsync(OmniSignature signature, string channel, string zone, CancellationToken cancellationToken = default);
}
