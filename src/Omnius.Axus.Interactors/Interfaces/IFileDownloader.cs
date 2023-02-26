using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors;

public interface IFileDownloader : IAsyncDisposable
{
    ValueTask<IEnumerable<FileDownloadingReport>> GetDownloadingFileReportsAsync(CancellationToken cancellationToken = default);
    ValueTask RegisterAsync(Seed seed, CancellationToken cancellationToken = default);
    ValueTask UnregisterAsync(Seed seed, CancellationToken cancellationToken = default);
    ValueTask<FileDownloaderConfig> GetConfigAsync(CancellationToken cancellationToken = default);
    ValueTask SetConfigAsync(FileDownloaderConfig config, CancellationToken cancellationToken = default);
}
