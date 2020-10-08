using System;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Xeus.Engines.Connectors.Primitives;
using Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Engines.Connectors
{
    public interface ITcpConnectorFactory
    {
        public ValueTask<ITcpConnector> CreateAsync(TcpConnectorOptions options, ISocks5ProxyClientFactory socks5ProxyClientFactory, IHttpProxyClientFactory httpProxyClientFactory, IUpnpClientFactory upnpClientFactory, IBytesPool bytesPool);
    }

    public interface ITcpConnector : IConnector, IAsyncDisposable
    {
    }
}
