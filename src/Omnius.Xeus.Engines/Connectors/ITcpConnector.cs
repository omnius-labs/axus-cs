using System;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Net.Proxies;
using Omnius.Core.Net.Upnp;
using Omnius.Xeus.Engines.Connectors.Primitives;
using Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Engines.Connectors
{
    public interface ITcpConnectorFactory
    {
        public ValueTask<ITcpConnector> CreateAsync(TcpConnectorOptions options, ISocks5ProxyClientFactory socks5ProxyClientFactory,
            IHttpProxyClientFactory httpProxyClientFactory, IUpnpClientFactory upnpClientFactory, IBytesPool bytesPool, CancellationToken cancellationToken = default);
    }

    public interface ITcpConnector : IConnector, IAsyncDisposable
    {
    }
}
