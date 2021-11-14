using Omnius.Core.Cryptography;
using Omnius.Xeus.Intaractors.Models;

namespace Omnius.Xeus.Intaractors;

public interface IProfileSubscriber
{
    ValueTask<IEnumerable<SubscribedProfileReport>> GetSubscribedProfileReportsAsync(CancellationToken cancellationToken = default);

    IAsyncEnumerable<Profile> FindAllAsync(CancellationToken cancellationToken = default);

    ValueTask<Profile?> FindOneAsync(OmniSignature signature, CancellationToken cancellationToken = default);

    ValueTask<ProfileSubscriberConfig> GetConfigAsync(CancellationToken cancellationToken = default);

    ValueTask SetConfigAsync(ProfileSubscriberConfig config, CancellationToken cancellationToken = default);
}
