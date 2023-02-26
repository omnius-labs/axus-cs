using Omnius.Axus.Engines.Models;
using Omnius.Core.Net;

namespace Omnius.Axus.Engines;

public interface IConnectionAcceptor : IAsyncDisposable
{
    ValueTask<ConnectionAcceptedResult?> AcceptAsync(CancellationToken cancellationToken = default);
    ValueTask<OmniAddress[]> GetListenEndpointsAsync(CancellationToken cancellationToken = default);
}
