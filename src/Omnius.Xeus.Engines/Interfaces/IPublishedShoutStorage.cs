using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Primitives;
using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines;

public interface IPublishedShoutStorage : IReadOnlyShoutStorage, IAsyncDisposable
{
    ValueTask<PublishedShoutStorageReport> GetReportAsync(CancellationToken cancellationToken = default);

    ValueTask PublishShoutAsync(Shout shout, string registrant, CancellationToken cancellationToken = default);

    ValueTask UnpublishShoutAsync(OmniSignature signature, string registrant, CancellationToken cancellationToken = default);
}
