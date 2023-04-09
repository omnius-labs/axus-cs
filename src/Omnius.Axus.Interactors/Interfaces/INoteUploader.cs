using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors;

public interface INoteUploader : IAsyncDisposable
{
    ValueTask<MemoUploaderConfig> GetConfigAsync(CancellationToken cancellationToken = default);
    ValueTask SetConfigAsync(MemoUploaderConfig config, CancellationToken cancellationToken = default);
}
