using Omnius.Core.Net;

namespace Omnius.Xeus.Engines;

public record TcpConnectionConnectorOptions
{
    public TcpConnectionConnectorOptions(TcpProxyOptions proxy)
    {
        this.Proxy = proxy;
    }

    public TcpProxyOptions Proxy { get; }
}

public record TcpProxyOptions
{
    public TcpProxyOptions(TcpProxyType type, OmniAddress address)
    {
        this.Type = type;
        this.Address = address;
    }

    public TcpProxyType Type { get; }

    public OmniAddress Address { get; }
}

public enum TcpProxyType : byte
{
    Unknown = 0,
    HttpProxy = 1,
    Socks5Proxy = 2,
}
