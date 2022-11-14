using Omnius.Axus.Messages;

namespace Omnius.Axus.Engines;

public interface INodeLocationsFetcher
{
    ValueTask<IEnumerable<NodeLocation>> FetchAsync(CancellationToken cancellationToken = default);
}
