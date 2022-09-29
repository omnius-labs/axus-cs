using Omnius.Axus.Models;

namespace Omnius.Axus.Engines;

public interface INodeLocationsFetcher
{
    ValueTask<IEnumerable<NodeLocation>> FetchAsync(CancellationToken cancellationToken = default);
}
