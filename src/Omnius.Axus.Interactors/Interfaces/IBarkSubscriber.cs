using Omnius.Axus.Interactors.Models;
using Omnius.Core.Cryptography;
using Omnius.Core.Pipelines;

namespace Omnius.Axus.Interactors;

public interface IBarkSubscriber : IAsyncDisposable
{
    IAsyncFuncListener<IEnumerable<OmniSignature>> OnGetTrustedSignatures { get; }

    ValueTask<IEnumerable<BarkMessageReport>> FindMessagesByTagAsync(string tag, CancellationToken cancellationToken = default);
    ValueTask<BarkMessageReport?> FindMessagesBySelfHashAsync(OmniHash selfHash, CancellationToken cancellationToken = default);
    ValueTask<BarkSubscriberConfig> GetConfigAsync(CancellationToken cancellationToken = default);
    ValueTask SetConfigAsync(BarkSubscriberConfig config, CancellationToken cancellationToken = default);
}
