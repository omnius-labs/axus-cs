using Omnius.Core.Net;

namespace Omnius.Axis.Engines;

public interface ISessionConnector : IAsyncDisposable
{
    ValueTask<ISession?> ConnectAsync(OmniAddress address, string scheme, CancellationToken cancellationToken = default);
}
