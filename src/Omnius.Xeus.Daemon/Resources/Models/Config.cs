using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Omnius.Xeus.Daemon.Resources.Models
{
    public class Config
    {
        public int Version { get; init; }

        public string? ListenAddress { get; init; }

        public EnginesConfig? Engines { get; init; }

        public static async ValueTask<Config?> LoadAsync(string configPath)
        {
            try
            {
                using var stream = new FileStream(configPath, FileMode.Open);
                var serializeOptions = new JsonSerializerOptions();
                return await JsonSerializer.DeserializeAsync<Config>(stream, serializeOptions);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class EnginesConfig
    {
        public string? WorkingDirectory { get; init; }

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
