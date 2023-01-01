using Omnius.Axus.Interactors.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors;

public interface IProfileDownloader : IAsyncDisposable
{
    ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default);
    ValueTask<ProfileReport?> FindProfileBySignatureAsync(OmniSignature signature, CancellationToken cancellationToken = default);
    ValueTask<ProfileDownloaderConfig> GetConfigAsync(CancellationToken cancellationToken = default);
    ValueTask SetConfigAsync(ProfileDownloaderConfig config, CancellationToken cancellationToken = default);
}
