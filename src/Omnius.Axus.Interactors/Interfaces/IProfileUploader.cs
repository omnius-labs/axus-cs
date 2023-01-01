using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors;

public interface IProfileUploader : IAsyncDisposable
{
    ValueTask<ProfileUploaderConfig> GetConfigAsync(CancellationToken cancellationToken = default);
    ValueTask SetConfigAsync(ProfileUploaderConfig config, CancellationToken cancellationToken = default);
}
