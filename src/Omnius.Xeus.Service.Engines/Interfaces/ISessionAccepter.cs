using Omnius.Core.Net;

namespace Omnius.Xeus.Service.Engines;

public interface ISessionAccepter : IAsyncDisposable
{
    ValueTask<ISession> AcceptAsync(string scheme, CancellationToken cancellationToken = default);

    ValueTask<OmniAddress[]> GetListenEndpointsAsync(CancellationToken cancellationToken = default);
}
