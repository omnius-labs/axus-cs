using Omnius.Axus.Engines.Models;
using Omnius.Axus.Messages;
using Omnius.Core.Pipelines;

namespace Omnius.Axus.Engines;

public interface INodeFinder : IAsyncDisposable
{
    IFuncListener<IEnumerable<ContentKey>> OnGetPushContentKeys { get; }
    IFuncListener<IEnumerable<ContentKey>> OnGetWantContentKeys { get; }

    ValueTask<IEnumerable<SessionReport>> GetSessionReportsAsync(CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<NodeLocation>> GetCloudNodeLocationsAsync(CancellationToken cancellationToken);
    ValueTask<NodeLocation> GetMyNodeLocationAsync(CancellationToken cancellationToken = default);
    ValueTask AddCloudNodeLocationsAsync(IEnumerable<NodeLocation> nodeLocations, CancellationToken cancellationToken = default);
    ValueTask<NodeLocation[]> FindNodeLocationsAsync(ContentKey ContentKey, CancellationToken cancellationToken = default);
}
