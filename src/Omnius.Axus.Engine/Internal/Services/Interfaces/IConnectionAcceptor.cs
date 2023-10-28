using Omnius.Axus.Engine.Internal.Models;
using Omnius.Core.Net;

namespace Omnius.Axus.Engine.Internal.Services;

public interface IConnectionAcceptor : IAsyncDisposable
{
    ValueTask<ConnectionAcceptedResult?> AcceptAsync(CancellationToken cancellationToken = default);
    ValueTask<OmniAddress[]> GetListenEndpointsAsync(CancellationToken cancellationToken = default);
}
