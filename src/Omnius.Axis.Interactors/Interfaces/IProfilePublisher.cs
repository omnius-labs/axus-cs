using Omnius.Axis.Interactors.Models;

namespace Omnius.Axis.Interactors;

public interface IProfilePublisher : IAsyncDisposable
{
    ValueTask<ProfilePublisherConfig> GetConfigAsync(CancellationToken cancellationToken = default);

    ValueTask SetConfigAsync(ProfilePublisherConfig config, CancellationToken cancellationToken = default);
}
