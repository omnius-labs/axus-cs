using Omnius.Xeus.Intaractors.Models;

namespace Omnius.Xeus.Intaractors;

public interface IFileDownloader
{
    ValueTask<IEnumerable<DownloadingFileReport>> GetDownloadingFileReportsAsync(CancellationToken cancellationToken = default);

    ValueTask RegisterAsync(Seed seed, CancellationToken cancellationToken = default);

    ValueTask UnregisterAsync(Seed seed, CancellationToken cancellationToken = default);
}
