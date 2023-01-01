using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors;

public interface ISeedUploader : IAsyncDisposable
{
    ValueTask<SeedUploaderConfig> GetConfigAsync(CancellationToken cancellationToken = default);
    ValueTask SetConfigAsync(SeedUploaderConfig config, CancellationToken cancellationToken = default);
}
