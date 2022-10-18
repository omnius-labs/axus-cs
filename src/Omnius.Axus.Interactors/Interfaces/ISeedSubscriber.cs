using Omnius.Axus.Interactors.Models;
using Omnius.Core.Cryptography;
using Omnius.Core.Pipelines;

namespace Omnius.Axus.Interactors;

public interface ISeedSubscriber : IAsyncDisposable
{
    IAsyncFuncListener<IEnumerable<OmniSignature>> OnGetTrustedSignatures { get; }

    ValueTask<FindSeedsResult> FindSeedsAsync(FindSeedsCondition condition, CancellationToken cancellationToken = default);
    ValueTask<SeedReport?> FindSeedsBySelfHashAsync(OmniHash selfHash, CancellationToken cancellationToken = default);
    ValueTask<BarkSubscriberConfig> GetConfigAsync(CancellationToken cancellationToken = default);
    ValueTask SetConfigAsync(BarkSubscriberConfig config, CancellationToken cancellationToken = default);
}

public class FindSeedsResult
{

}

public class FindSeedsCondition
{

}
