using Omnius.Core.Net;

namespace Omnius.Axis.Engines;

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
    None = 0,
    HttpProxy = 1,
    Socks5Proxy = 2,
}
