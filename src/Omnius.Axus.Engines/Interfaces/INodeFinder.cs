using Omnius.Axus.Models;

namespace Omnius.Axus.Engines;

public interface INodeFinder : IAsyncDisposable
{
    INodeFinderEvents GetEvents();
    ValueTask<IEnumerable<SessionReport>> GetSessionReportsAsync(CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<NodeLocation>> GetCloudNodeLocationsAsync(CancellationToken cancellationToken);
    ValueTask<NodeLocation> GetMyNodeLocationAsync(CancellationToken cancellationToken = default);
    ValueTask AddCloudNodeLocationsAsync(IEnumerable<NodeLocation> nodeLocations, CancellationToken cancellationToken = default);
    ValueTask<NodeLocation[]> FindNodeLocationsAsync(ContentClue contentClue, CancellationToken cancellationToken = default);
}
