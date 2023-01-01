using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors;

public interface ISeedDownloader
{
    ValueTask<FindSeedsResult> FindSeedsAsync(FindSeedsCondition condition, CancellationToken cancellationToken = default);
    ValueTask<SeedDownloaderConfig> GetConfigAsync(CancellationToken cancellationToken = default);
    ValueTask SetConfigAsync(SeedDownloaderConfig config, CancellationToken cancellationToken = default);
}
