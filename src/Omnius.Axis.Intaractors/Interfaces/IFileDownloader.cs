using Omnius.Axis.Intaractors.Models;

namespace Omnius.Axis.Intaractors;

public interface IFileDownloader : IAsyncDisposable
{
    ValueTask<IEnumerable<DownloadingFileReport>> GetDownloadingFileReportsAsync(CancellationToken cancellationToken = default);

    ValueTask RegisterAsync(Seed seed, CancellationToken cancellationToken = default);

    ValueTask UnregisterAsync(Seed seed, CancellationToken cancellationToken = default);

    ValueTask<FileDownloaderConfig> GetConfigAsync(CancellationToken cancellationToken = default);

    ValueTask SetConfigAsync(FileDownloaderConfig config, CancellationToken cancellationToken = default);
}
