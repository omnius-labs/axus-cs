using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Network;
using Omnius.Core.Network.Caps;
using Omnius.Core.Network.Connections;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;

namespace Omnius.Xeus.Components.Connectors
{
    public readonly struct ConnectorAcceptResult
    {
        public ConnectorAcceptResult(IConnection connection, OmniAddress address)
        {
            this.Connection = connection;
            this.Address = address;
        }

        public IConnection Connection { get; }
        public OmniAddress Address { get; }
    }

    public interface IConnector
    {
        ValueTask<IConnection?> ConnectAsync(OmniAddress address, string serviceId, CancellationToken cancellationToken = default);
        ValueTask<ConnectorAcceptResult> AcceptAsync(string serviceId, CancellationToken cancellationToken = default);
        ValueTask<OmniAddress[]> GetListenEndpointsAsync(CancellationToken cancellationToken = default);
    }
}
