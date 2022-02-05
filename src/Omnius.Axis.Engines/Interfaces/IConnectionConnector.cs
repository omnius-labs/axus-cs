using Omnius.Core.Net;
using Omnius.Core.Net.Connections;

namespace Omnius.Axis.Engines;

public interface IConnectionConnector : IAsyncDisposable
{
    ValueTask<IConnection?> ConnectAsync(OmniAddress address, CancellationToken cancellationToken = default);
}
