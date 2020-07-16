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

namespace Omnius.Xeus.Service.Drivers
{
    public readonly struct ConnectionControllerAcceptResult
    {
        public ConnectionControllerAcceptResult(IConnection connection, OmniAddress address)
        {
            this.Connection = connection;
            this.Address = address;
        }

        public IConnection Connection { get; }
        public OmniAddress Address { get; }
    }

    public interface IConnectionControllerFactory
    {
        public ValueTask<IConnectionController> CreateAsync(ConnectionControllerOptions options, ISocks5ProxyClientFactory socks5ProxyClientFactory, IHttpProxyClientFactory httpProxyClientFactory, IUpnpClientFactory upnpClientFactory, IBytesPool bytesPool);
    }

    public interface IConnectionController : IAsyncDisposable
    {
        ValueTask<IConnection?> ConnectAsync(OmniAddress address, string serviceId, CancellationToken cancellationToken = default);
        ValueTask<ConnectionControllerAcceptResult> AcceptAsync(string serviceId, CancellationToken cancellationToken = default);
        ValueTask<OmniAddress[]> GetListenEndpointsAsync(CancellationToken cancellationToken = default);
    }
}
