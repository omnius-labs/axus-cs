using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors;

public interface IFileDownloader : IAsyncDisposable
{
    ValueTask<IEnumerable<DownloadingFileReport>> GetDownloadingFileReportsAsync(CancellationToken cancellationToken = default);
    ValueTask RegisterAsync(FileSeed fileSeed, CancellationToken cancellationToken = default);
    ValueTask UnregisterAsync(FileSeed fileSeed, CancellationToken cancellationToken = default);
    ValueTask<FileDownloaderConfig> GetConfigAsync(CancellationToken cancellationToken = default);
    ValueTask SetConfigAsync(FileDownloaderConfig config, CancellationToken cancellationToken = default);
}
