using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors;

public interface ISeedPublisher : IAsyncDisposable
{
    ValueTask<SeedPublisherConfig> GetConfigAsync(CancellationToken cancellationToken = default);
    ValueTask SetConfigAsync(SeedPublisherConfig config, CancellationToken cancellationToken = default);
}
