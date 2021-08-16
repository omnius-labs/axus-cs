using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Proxies;
using Omnius.Core.Net.Upnp;
using Omnius.Core.Tasks;

namespace Omnius.Xeus.Engines
{
    public record TcpConnectionConnectorOptions
    {
        public TcpProxyOptions? Proxy { get; init; }

        public IBandwidthLimiter? SenderBandwidthLimiter { get; init; }

        public IBandwidthLimiter? ReceiverBandwidthLimiter { get; init; }

        public ISocks5ProxyClientFactory? Socks5ProxyClientFactory { get; init; }

        public IHttpProxyClientFactory? HttpProxyClientFactory { get; init; }

        public IBatchActionDispatcher? BatchActionDispatcher { get; init; }
    }

    public record TcpProxyOptions
    {
        public TcpProxyType Type { get; init; }

        public OmniAddress? Address { get; init; }
    }

    public enum TcpProxyType : byte
    {
        Unknown = 0,
        HttpProxy = 1,
        Socks5Proxy = 2,
    }
}
