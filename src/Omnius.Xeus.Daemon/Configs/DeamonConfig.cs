using System.IO;
using System.Net;
using Omnius.Core.Network;
using Omnius.Xeus.Daemon.Internal;

namespace Omnius.Xeus.Daemon.Configs
{
    public class DaemonConfig
    {
        public string? ListenAddress { get; init; }

        public EnginesConfig? Engines { get; init; }

        private const string ConfigFileName = "daemon.yaml";

        public static DaemonConfig LoadConfig(string configDirectoryPath, string stateDirectoryPath)
        {
            InitConfig(configDirectoryPath, stateDirectoryPath);

            var configPath = Path.Combine(configDirectoryPath, ConfigFileName);
            var config = YamlHelper.ReadFile<DaemonConfig>(configPath);
            return config;
        }

        private static void InitConfig(string configDirectoryPath, string stateDirectoryPath)
        {
            if (!Directory.Exists(configDirectoryPath))
            {
                Directory.CreateDirectory(configDirectoryPath);
            }

            var config = CreateInitConfig(stateDirectoryPath);

            YamlHelper.WriteFile(Path.Combine(configDirectoryPath, ConfigFileName), config);
        }

        private static DaemonConfig CreateInitConfig(string workingDirectory)
        {
            var config = new DaemonConfig()
            {
                ListenAddress = (string?)OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 32321),
                Engines = new EnginesConfig()
                {
                    WorkingDirectory = workingDirectory,
                    Connectors = new ConnectorsConfig()
                    {
                        TcpConnector = new TcpConnectorConfig()
                        {
                            Bandwidth = new BandwidthConfig()
                            {
                                MaxSendBytesPerSeconds = 1024 * 1024 * 32,
                                MaxReceiveBytesPerSeconds = 1024 * 1024 * 32,
                            },
                            Connecting = new TcpConnectingConfig()
                            {
                                Enabled = true,
                                Proxy = null,
                            },
                            Accepting = new TcpAcceptingConfig()
                            {
                                Enabled = true,
                                ListenAddresses = new string[] { OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 32320).ToString() },
                                UseUpnp = true,
                            },
                        },
                    },
                    Exchangers = new ExchangersConfig()
                    {
                        ContentExchanger = new ContentExchangerConfig()
                        {
                            MaxConnectionCount = 32,
                        },
                        DeclaredMessageExchanger = new DeclaredMessageConfig()
                        {
                            MaxConnectionCount = 32,
                        },
                    },
                },
            };

            return config;
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
