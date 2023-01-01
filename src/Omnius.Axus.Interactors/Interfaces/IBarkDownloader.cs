using Omnius.Axus.Interactors.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors;

public interface IBarkDownloader : IAsyncDisposable
{
    ValueTask<IEnumerable<BarkMessageReport>> FindMessagesByTagAsync(string tag, CancellationToken cancellationToken = default);
    ValueTask<BarkMessageReport?> FindMessageBySelfHashAsync(OmniHash selfHash, CancellationToken cancellationToken = default);
    ValueTask<BarkDownloaderConfig> GetConfigAsync(CancellationToken cancellationToken = default);
    ValueTask SetConfigAsync(BarkDownloaderConfig config, CancellationToken cancellationToken = default);
}
