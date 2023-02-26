using Omnius.Axus.Interactors.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors;

public interface IMemoDownloader : IAsyncDisposable
{
    ValueTask<IEnumerable<MemoReport>> FindMessagesByTagAsync(string tag, CancellationToken cancellationToken = default);
    ValueTask<MemoReport?> FindMessageBySelfHashAsync(OmniHash selfHash, CancellationToken cancellationToken = default);
    ValueTask<MemoDownloaderConfig> GetConfigAsync(CancellationToken cancellationToken = default);
    ValueTask SetConfigAsync(MemoDownloaderConfig config, CancellationToken cancellationToken = default);
}
