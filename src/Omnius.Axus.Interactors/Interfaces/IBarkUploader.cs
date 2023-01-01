using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors;

public interface IBarkUploader : IAsyncDisposable
{
    ValueTask<BarkUploaderConfig> GetConfigAsync(CancellationToken cancellationToken = default);
    ValueTask SetConfigAsync(BarkUploaderConfig config, CancellationToken cancellationToken = default);
}
