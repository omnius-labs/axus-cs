using Omnius.Axus.Core.Engine.Models;
using Omnius.Core.Net;

namespace Omnius.Axus.Core.Engine.Services;

public interface IConnectionAcceptor : IAsyncDisposable
{
    ValueTask<ConnectionAcceptedResult?> AcceptAsync(CancellationToken cancellationToken = default);
    ValueTask<OmniAddress[]> GetListenEndpointsAsync(CancellationToken cancellationToken = default);
}
