using System;
using System.Threading.Tasks;
using Omnius.Core.Utils;
using Omnius.Xeus.Utils;

namespace Omnius.Xeus.Daemon.Configuration
{
    public class AppConfig
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public int Version { get; init; }

        public string? ListenAddress { get; init; }

        public EnginesConfig? Engines { get; init; }

        public static async ValueTask<AppConfig?> LoadAsync(string configPath)
        {
            try
            {
                return YamlHelper.ReadFile<AppConfig>(configPath);
            }
            catch (Exception e)
            {
                _logger.Debug(e);
                return null;
            }
        }

        public async ValueTask SaveAsync(string configPath)
        {
            YamlHelper.WriteFile(configPath, this);
        }
    }

    public class EnginesConfig
    {
        public ConnectorsConfig? Connectors { get; init; }

        public ExchangersConfig? Exchangers { get; init; }
    }

    public class ConnectorsConfig
    {
        public TcpConnectorConfig? TcpConnector { get; init; }
    }

    public class TcpConnectorConfig
    {
        public BandwidthConfig? Bandwidth { get; init; }

        public TcpConnectingConfig? Connecting { get; init; }

        public TcpAcceptingConfig? Accepting { get; init; }
    }

    public class BandwidthConfig
    {
        public uint MaxSendBytesPerSeconds { get; init; }

        public uint MaxReceiveBytesPerSeconds { get; init; }
    }

    public class TcpConnectingConfig
    {
        public bool Enabled { get; init; }

        public TcpProxyConfig? Proxy { get; init; }
    }

    public class TcpProxyConfig
    {
        public TcpProxyType Type { get; init; }

        public string? Address { get; init; }
    }

    public enum TcpProxyType : byte
    {
        Unknown = 0,
        HttpProxy = 1,
        Socks5Proxy = 2,
    }

    public class TcpAcceptingConfig
    {
        public bool Enabled { get; init; }

        public string[]? ListenAddresses { get; init; }

        public bool UseUpnp { get; init; }
    }

    public class ExchangersConfig
    {
        public ContentExchangerConfig? ContentExchanger { get; init; }

        public DeclaredMessageConfig? DeclaredMessageExchanger { get; init; }
    }

    public class ContentExchangerConfig
    {
        public uint MaxConnectionCount { get; init; }
    }

    public class DeclaredMessageConfig
    {
        public uint MaxConnectionCount { get; init; }
    }
}
