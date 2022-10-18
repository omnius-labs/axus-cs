using Omnius.Axus.Engines.Models;
using Omnius.Core.Net;

namespace Omnius.Axus.Engines;

public interface IConnectionAccepter : IAsyncDisposable
{
    ValueTask<ConnectionAcceptedResult?> AcceptAsync(CancellationToken cancellationToken = default);
    ValueTask<OmniAddress[]> GetListenEndpointsAsync(CancellationToken cancellationToken = default);
}
