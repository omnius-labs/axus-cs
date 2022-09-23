using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors;

public interface IBarkPublisher : IAsyncDisposable
{
    ValueTask<BarkPublisherConfig> GetConfigAsync(CancellationToken cancellationToken = default);

    ValueTask SetConfigAsync(BarkPublisherConfig config, CancellationToken cancellationToken = default);
}