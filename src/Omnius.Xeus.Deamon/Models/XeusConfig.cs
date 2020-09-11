using System.Threading;
using System.Threading.Tasks;
using Omnius.Xeus.Components.Models;
using Omnius.Xeus.Rpc;

namespace Omnius.Xeus.Deamon.Models
{
    public record XeusConfig
    {
        public string? WorkingDirectory;

        public ConnectorsConfig? Connectors;

        public record ConnectorsConfig
        {
            public TcpConnectorConfig? TcpConnector;
        }

        public record TcpConnectorConfig
        {
            public BandwidthConfig? Bandwidth;
            public TcpConnectingConfig? Connecting;
            public TcpAcceptingConfig? Accepting;
        }

        public record BandwidthConfig
        {
            public uint MaxSendBytesPerSeconds;
            public uint MaxReceiveBytesPerSeconds;
        }

        public record TcpConnectingConfig
        {
            public bool Enabled;
            public TcpProxyConfig? Proxy;
        }

        public record TcpProxyConfig
        {
            public TcpProxyType Type;
            public string? Address;
        }

        public enum TcpProxyType : byte
        {
            Unknown = 0,
            HttpProxy = 1,
            Socks5Proxy = 2,
        }

        public record TcpAcceptingConfig
        {
            public bool Enabled;
            public string[]? ListenAddresses;
            public bool UseUpnp;
        }
    }
}
