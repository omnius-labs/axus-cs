using System;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Xeus.Components.Connectors.Primitives;
using Omnius.Xeus.Components.Models;

namespace Omnius.Xeus.Components.Connectors
{
    public interface ITcpConnectorFactory
    {
        public ValueTask<ITcpConnector> CreateAsync(TcpConnectorOptions options, ISocks5ProxyClientFactory socks5ProxyClientFactory, IHttpProxyClientFactory httpProxyClientFactory, IUpnpClientFactory upnpClientFactory, IBytesPool bytesPool);
    }

    public interface ITcpConnector : IConnector, IAsyncDisposable
    {
    }
}
