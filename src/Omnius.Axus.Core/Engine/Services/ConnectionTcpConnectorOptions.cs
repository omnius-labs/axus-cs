using Omnius.Core.Net;

namespace Omnius.Axus.Core.Engine.Services;

public record ConnectionTcpConnectorOptions
{
    public required TcpProxyOptions Proxy { get; init; }
}

public record TcpProxyOptions
{
    public required TcpProxyType Type { get; init; }
    public required OmniAddress Address { get; init; }
}

public enum TcpProxyType : byte
{
    None = 0,
    HttpProxy = 1,
    Socks5Proxy = 2,
}
