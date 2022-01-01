using Omnius.Axis.Intaractors.Models;

namespace Omnius.Axis.Intaractors;

public interface IProfilePublisher
{
    ValueTask PublishAsync(ProfileContent profileContent, CancellationToken cancellationToken = default);

    ValueTask<ProfilePublisherConfig> GetConfigAsync(CancellationToken cancellationToken = default);

    ValueTask SetConfigAsync(ProfilePublisherConfig config, CancellationToken cancellationToken = default);
}
