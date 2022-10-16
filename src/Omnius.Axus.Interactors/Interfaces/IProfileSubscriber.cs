using Omnius.Axus.Interactors.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors;

public interface IProfileSubscriber : IAsyncDisposable
{
    ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<ProfileReport>> FindProfilesAsync(CancellationToken cancellationToken = default);
    ValueTask<ProfileReport?> FindProfileBySignatureAsync(OmniSignature signature, CancellationToken cancellationToken = default);
    ValueTask<ProfileSubscriberConfig> GetConfigAsync(CancellationToken cancellationToken = default);
    ValueTask SetConfigAsync(ProfileSubscriberConfig config, CancellationToken cancellationToken = default);
}
