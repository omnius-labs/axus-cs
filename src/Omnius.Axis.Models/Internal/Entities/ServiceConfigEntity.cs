using Omnius.Core.Net;

namespace Omnius.Axis.Models.Internal.Entities;

internal record ServiceConfigEntity
{
    public BandwidthConfigEntity? Bandwidth { get; set; }

    public TcpConnectorConfigEntity? TcpConnector { get; set; }

    public TcpAccepterConfigEntity? TcpAccepter { get; set; }

    public static ServiceConfigEntity Import(ServiceConfig value)
    {
        return new ServiceConfigEntity()
        {
            Bandwidth = (value.Bandwidth is not null) ? BandwidthConfigEntity.Import(value.Bandwidth) : null,
            TcpConnector = (value.TcpConnector is not null) ? TcpConnectorConfigEntity.Import(value.TcpConnector) : null,
            TcpAccepter = (value.TcpAccepter is not null) ? TcpAccepterConfigEntity.Import(value.TcpAccepter) : null,
        };
    }

    public ServiceConfig Export()
    {
        return new ServiceConfig(this.Bandwidth?.Export(), this.TcpConnector?.Export(), this.TcpAccepter?.Export());
    }
}

internal record BandwidthConfigEntity
{
    public int MaxSendBytesPerSeconds { get; set; }

    public int MaxReceiveBytesPerSeconds { get; set; }

    public static BandwidthConfigEntity Import(BandwidthConfig value)
    {
        return new BandwidthConfigEntity()
        {
            MaxSendBytesPerSeconds = value.MaxSendBytesPerSeconds,
            MaxReceiveBytesPerSeconds = value.MaxReceiveBytesPerSeconds,
        };
    }

    public BandwidthConfig Export()
    {
        return new BandwidthConfig(this.MaxSendBytesPerSeconds, this.MaxSendBytesPerSeconds);
    }
}

internal record TcpConnectorConfigEntity
{
    public bool IsEnabled { get; set; }

    public TcpProxyConfigEntity? TcpProxy { get; set; }

    public static TcpConnectorConfigEntity Import(TcpConnectorConfig value)
    {
        return new TcpConnectorConfigEntity()
        {
            IsEnabled = value.IsEnabled,
            TcpProxy = (value.Proxy is not null) ? TcpProxyConfigEntity.Import(value.Proxy) : null,
        };
    }

    public TcpConnectorConfig Export()
    {
        return new TcpConnectorConfig(this.IsEnabled, this.TcpProxy?.Export());
    }
}

internal record TcpProxyConfigEntity
{
    public string? Type { get; set; }

    public string? Address { get; set; }

    public static TcpProxyConfigEntity Import(TcpProxyConfig value)
    {
        return new TcpProxyConfigEntity()
        {
            Type = value.Type.ToString(),
            Address = value.Address.ToString(),
        };
    }

    public TcpProxyConfig Export()
    {
        return new TcpProxyConfig(
            (this.Type is not null) ? Enum.Parse<TcpProxyType>(this.Type) : TcpProxyType.None,
            OmniAddress.Parse(this.Address));
    }
}

internal record TcpAccepterConfigEntity
{
    public bool IsEnabled { get; set; }

    public bool UseUpnp { get; set; }

    public string? ListenAddress { get; set; }

    public static TcpAccepterConfigEntity Import(TcpAccepterConfig value)
    {
        return new TcpAccepterConfigEntity()
        {
            IsEnabled = value.IsEnabled,
            UseUpnp = value.UseUpnp,
            ListenAddress = value.ListenAddress.ToString(),
        };
    }

    public TcpAccepterConfig Export()
    {
        return new TcpAccepterConfig(this.IsEnabled, this.UseUpnp, OmniAddress.Parse(this.ListenAddress));
    }
}
