using Omnius.Core.Net;
using Omnius.Core.Net.Connections;

namespace Omnius.Xeus.Service.Engines;

public interface IConnectionConnector
{
    ValueTask<IConnection?> ConnectAsync(OmniAddress address, CancellationToken cancellationToken = default);
}
