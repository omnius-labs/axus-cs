using Omnius.Axis.Engines.Primitives;
using Omnius.Axis.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axis.Engines;

public interface IPublishedShoutStorage : IReadOnlyShoutStorage, IAsyncDisposable
{
    ValueTask<IEnumerable<PublishedShoutReport>> GetPublishedShoutReportsAsync(CancellationToken cancellationToken = default);

    ValueTask PublishShoutAsync(Shout shout, string registrant, CancellationToken cancellationToken = default);

    ValueTask UnpublishShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default);
}
