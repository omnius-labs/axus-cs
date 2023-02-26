using Omnius.Axus.Engines.Primitives;
using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines;

public interface IShoutPublisherStorage : IReadOnlyShoutStorage, IAsyncDisposable
{
    ValueTask<IEnumerable<PublishedShoutReport>> GetPublishedShoutReportsAsync(string zone, CancellationToken cancellationToken = default);
    ValueTask PublishShoutAsync(Shout shout, string zone, CancellationToken cancellationToken = default);
    ValueTask UnpublishShoutAsync(OmniSignature signature, string channel, string zone, CancellationToken cancellationToken = default);
}
