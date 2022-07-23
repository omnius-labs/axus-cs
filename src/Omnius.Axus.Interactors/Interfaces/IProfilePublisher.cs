using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors;

public interface IProfilePublisher : IAsyncDisposable
{
    ValueTask<ProfilePublisherConfig> GetConfigAsync(CancellationToken cancellationToken = default);

    ValueTask SetConfigAsync(ProfilePublisherConfig config, CancellationToken cancellationToken = default);
}
