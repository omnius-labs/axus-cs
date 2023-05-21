using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors;

public interface INoteUploader : IAsyncDisposable
{
    ValueTask<NoteUploaderConfig> GetConfigAsync(CancellationToken cancellationToken = default);
    ValueTask SetConfigAsync(NoteUploaderConfig config, CancellationToken cancellationToken = default);
}
