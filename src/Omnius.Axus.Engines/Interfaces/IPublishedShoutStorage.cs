using Omnius.Axus.Engines.Primitives;
using Omnius.Axus.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines;

public interface IPublishedShoutStorage : IReadOnlyShoutStorage, IAsyncDisposable
{
    ValueTask<IEnumerable<PublishedShoutReport>> GetPublishedShoutReportsAsync(CancellationToken cancellationToken = default);

    ValueTask PublishShoutAsync(Shout shout, string author, CancellationToken cancellationToken = default);

    ValueTask UnpublishShoutAsync(OmniSignature signature, string channel, string author, CancellationToken cancellationToken = default);
}