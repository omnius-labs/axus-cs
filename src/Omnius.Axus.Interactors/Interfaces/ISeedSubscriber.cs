using Omnius.Axus.Interactors.Models;
using Omnius.Core.Cryptography;
using Omnius.Core.Pipelines;

namespace Omnius.Axus.Interactors;

public interface ISeedSubscriber
{
    IAsyncFuncListener<IEnumerable<OmniSignature>> OnGetTrustedSignatures { get; }

    ValueTask<FindSeedsResult> FindSeedsAsync(FindSeedsCondition condition, CancellationToken cancellationToken = default);
    ValueTask<SeedSubscriberConfig> GetConfigAsync(CancellationToken cancellationToken = default);
    ValueTask SetConfigAsync(SeedSubscriberConfig config, CancellationToken cancellationToken = default);
}
