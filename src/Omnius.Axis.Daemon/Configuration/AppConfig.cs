using System.Net;
using Omnius.Core.Net;
using Omnius.Core.Utils;

namespace Omnius.Axis.Daemon.Configuration;

public class AppConfig
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    public int Version { get; init; }

    public RunMode Mode { get; init; }

    public string? DatabaseDirectoryPath { get; init; }

    public string? LogsDirectoryPath { get; init; }

    public string? ListenAddress { get; init; }

    public BandwidthConfig? Bandwidth { get; init; }

    public SessionConnectorConfig? SessionConnector { get; init; }

    public SessionAccepterConfig? SessionAccepter { get; init; }

    public static async ValueTask<AppConfig> LoadAsync(string configPath)
    {
        AppConfig? result = null;

        try
        {
            result = YamlHelper.ReadFile<AppConfig>(configPath);
        }
        catch (Exception e)
        {
            _logger.Debug(e);
        }

        result ??= new AppConfig()
        {
            Version = 1,
            Mode = RunMode.Release,
            DatabaseDirectoryPath = "db",
            LogsDirectoryPath = "logs",
            ListenAddress = OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 32321).ToString(),
            Bandwidth = new BandwidthConfig()
            {
                MaxSendBytesPerSeconds = 1024 * 1024 * 32,
                MaxReceiveBytesPerSeconds = 1024 * 1024 * 32,
            },
            SessionConnector = new SessionConnectorConfig()
            {
                TcpConnector = new TcpConnectorConfig()
                {
                    Proxy = new TcpProxyConfig()
                    {
                        Type = TcpProxyType.None,
                    },
                },
            },
            SessionAccepter = new SessionAccepterConfig()
            {
                TcpAccepter = new TcpAccepterConfig()
                {
                    UseUpnp = true,
                    ListenAddress = OmniAddress.CreateTcpEndpoint(IPAddress.Any, (ushort)Random.Shared.Next(10000, 60000)).ToString(),
                },
            },
        };

        YamlHelper.WriteFile(configPath, result);

        return result;
    }

    public async ValueTask SaveAsync(string configPath)
    {
        YamlHelper.WriteFile(configPath, this);
    }
}

public enum RunMode
{
    Debug,
    Release,
}

public class BandwidthConfig
{
    public int MaxSendBytesPerSeconds { get; init; }

    public int MaxReceiveBytesPerSeconds { get; init; }
}

public class SessionConnectorConfig
{
    public TcpConnectorConfig? TcpConnector { get; init; }
}

public class SessionAccepterConfig
{
    public TcpAccepterConfig? TcpAccepter { get; init; }
}

public class TcpConnectorConfig
{
    public TcpProxyConfig? Proxy { get; init; }
}

public class TcpProxyConfig
{
    public TcpProxyType Type { get; init; }

    public string? Address { get; init; }
}

public enum TcpProxyType : byte
{
    None = 0,
    HttpProxy = 1,
    Socks5Proxy = 2,
}

public class TcpAccepterConfig
{
    public bool UseUpnp { get; init; }

    public string? ListenAddress { get; init; }
}
