using Omnius.Axus.Interactors.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors;

public interface INoteDownloader : IAsyncDisposable
{
    ValueTask<IEnumerable<NoteReport>> FindMessagesByTagAsync(string tag, CancellationToken cancellationToken = default);
    ValueTask<NoteReport?> FindMessageBySelfHashAsync(OmniHash selfHash, CancellationToken cancellationToken = default);
    ValueTask<NoteDownloaderConfig> GetConfigAsync(CancellationToken cancellationToken = default);
    ValueTask SetConfigAsync(NoteDownloaderConfig config, CancellationToken cancellationToken = default);
}
