using Omnius.Core.Net;
using Omnius.Core.Net.Connections;

namespace Omnius.Axus.Engine.Internal.Services;

public interface IConnectionConnector : IAsyncDisposable
{
    ValueTask<IConnection?> ConnectAsync(OmniAddress address, CancellationToken cancellationToken = default);
}
