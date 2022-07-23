using Omnius.Axus.Interactors.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors;

public interface IBarkSubscriber : IAsyncDisposable
{
    ValueTask<IEnumerable<BarkMessage>> FindByTagAsync(string tag, CancellationToken cancellationToken = default);

    ValueTask<BarkMessage?> FindBySelfHashAsync(string tag, OmniHash hash, CancellationToken cancellationToken = default);

    ValueTask<BarkSubscriberConfig> GetConfigAsync(CancellationToken cancellationToken = default);

    ValueTask SetConfigAsync(BarkSubscriberConfig config, CancellationToken cancellationToken = default);
}
