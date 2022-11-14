using Omnius.Axus.Engines.Models;
using Omnius.Axus.Messages;
using Omnius.Core.Pipelines;

namespace Omnius.Axus.Engines;

public interface INodeFinder : IAsyncDisposable
{
    IFuncListener<IEnumerable<ContentClue>> OnGetPushContentClues { get; }
    IFuncListener<IEnumerable<ContentClue>> OnGetWantContentClues { get; }

    ValueTask<IEnumerable<SessionReport>> GetSessionReportsAsync(CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<NodeLocation>> GetCloudNodeLocationsAsync(CancellationToken cancellationToken);
    ValueTask<NodeLocation> GetMyNodeLocationAsync(CancellationToken cancellationToken = default);
    ValueTask AddCloudNodeLocationsAsync(IEnumerable<NodeLocation> nodeLocations, CancellationToken cancellationToken = default);
    ValueTask<NodeLocation[]> FindNodeLocationsAsync(ContentClue contentClue, CancellationToken cancellationToken = default);
}
