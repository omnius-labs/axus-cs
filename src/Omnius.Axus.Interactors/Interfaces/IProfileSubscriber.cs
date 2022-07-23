using Omnius.Axus.Interactors.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors;

public interface IProfileSubscriber : IAsyncDisposable
{
    ValueTask<IEnumerable<ProfileMessage>> FindAllAsync(CancellationToken cancellationToken = default);

    ValueTask<ProfileMessage?> FindBySignatureAsync(OmniSignature signature, CancellationToken cancellationToken = default);

    ValueTask<ProfileSubscriberConfig> GetConfigAsync(CancellationToken cancellationToken = default);

    ValueTask SetConfigAsync(ProfileSubscriberConfig config, CancellationToken cancellationToken = default);
}
