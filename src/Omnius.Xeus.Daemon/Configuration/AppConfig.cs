using System;
using System.Threading.Tasks;
using Omnius.Core.Utils;

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
        public BandwidthConfig? Bandwidth { get; init; }

        public SessionConnectorConfig? SessionConnector { get; init; }

        public SessionAccepterConfig? SessionAccepter { get; init; }

        public NodeFinderConfig? NodeFinder { get; init; }

        public FileExchangerConfig? FileExchanger { get; init; }

        public ShoutExchangerConfig? ShoutExchanger { get; init; }
    }

    public class BandwidthConfig
    {
        public int MaxSendBytesPerSeconds { get; init; }

        public int MaxReceiveBytesPerSeconds { get; init; }
    }

    public class SessionConnectorConfig
    {
        public TcpConnectorConfig[]? TcpConnectors { get; init; }
    }

    public class SessionAccepterConfig
    {
        public TcpAccepterConfig[]? TcpAccepters { get; init; }
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
        Unknown = 0,
        HttpProxy = 1,
        Socks5Proxy = 2,
    }

    public class TcpAccepterConfig
    {
        public bool UseUpnp { get; init; }

        public string? ListenAddress { get; init; }
    }

    public class NodeFinderConfig
    {
        public uint MaxSessionCount { get; init; }
    }

    public class FileExchangerConfig
    {
        public uint MaxSessionCount { get; init; }
    }

    public class ShoutExchangerConfig
    {
        public uint MaxSessionCount { get; init; }
    }
}
