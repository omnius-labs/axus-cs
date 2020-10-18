
namespace Omnius.Xeus.Daemon.Configs
{
    public record DaemonConfig
    {
        public string? ListenAddress;
        public EnginesConfig? Engines;
    }

    public record EnginesConfig
    {
        public string? WorkingDirectory;
        public ConnectorsConfig? Connectors;
        public ExchangersConfig? Exchangers;
    }

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

    public record ExchangersConfig
    {
        public ContentExchangerConfig? ContentExchanger;
        public DeclaredMessageConfig? DeclaredMessageExchanger;
    }

    public record ContentExchangerConfig
    {
        public uint MaxConnectionCount;
    }

    public record DeclaredMessageConfig
    {
        public uint MaxConnectionCount;
    }
}
