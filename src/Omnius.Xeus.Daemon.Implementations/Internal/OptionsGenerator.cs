using System;
using System.Linq;
using Omnius.Core.Net;
using EnginesModels = Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Daemon.Internal
{
    internal class OptionsGenerator
    {
        private static OmniAddress? CreateAddress(string? value)
        {
            if (value is null) return null;

            return new OmniAddress(value);
        }

        public static EnginesModels.TcpConnectorOptions GenTcpConnectorOptions(Configuration.EnginesConfig config)
        {
            static EnginesModels.TcpConnectingOptions GenTcpConnectingOptions(Configuration.EnginesConfig config)
            {
                return new EnginesModels.TcpConnectingOptions(
                    config.Connectors?.TcpConnector?.Connecting?.Enabled ?? false,
                    new EnginesModels.TcpProxyOptions(
                        config.Connectors?.TcpConnector?.Connecting?.Proxy?.Type switch
                        {
                            Configuration.TcpProxyType.HttpProxy => EnginesModels.TcpProxyType.HttpProxy,
                            Configuration.TcpProxyType.Socks5Proxy => EnginesModels.TcpProxyType.Socks5Proxy,
                            _ => EnginesModels.TcpProxyType.Unknown,
                        },
                        CreateAddress(config?.Connectors?.TcpConnector?.Connecting?.Proxy?.Address)));
            }

            static EnginesModels.TcpAcceptingOptions GenTcpAcceptingOptions(Configuration.EnginesConfig config)
            {
                return new EnginesModels.TcpAcceptingOptions(
                    config?.Connectors?.TcpConnector?.Accepting?.Enabled ?? false,
                    config?.Connectors?.TcpConnector?.Accepting?.ListenAddresses?.Select(n => new OmniAddress(n))?.ToArray() ?? Array.Empty<OmniAddress>(),
                    config?.Connectors?.TcpConnector?.Accepting?.UseUpnp ?? false);
            }

            static EnginesModels.BandwidthOptions GenBandwidthOptions(Configuration.EnginesConfig config)
            {
                return new EnginesModels.BandwidthOptions(
                    config?.Connectors?.TcpConnector?.Bandwidth?.MaxSendBytesPerSeconds ?? 1024 * 1024 * 32,
                    config?.Connectors?.TcpConnector?.Bandwidth?.MaxReceiveBytesPerSeconds ?? 1024 * 1024 * 32);
            }

            return new EnginesModels.TcpConnectorOptions(
                GenTcpConnectingOptions(config),
                GenTcpAcceptingOptions(config),
                GenBandwidthOptions(config));
        }

        public static EnginesModels.CkadMediatorOptions GenCkadMediatorOptions(string configDirectoryPath, Configuration.EnginesConfig config)
        {
            return new EnginesModels.CkadMediatorOptions(configDirectoryPath, 10);
        }

        public static EnginesModels.ContentExchangerOptions GenContentExchangerOptions(string configDirectoryPath, Configuration.EnginesConfig config)
        {
            return new EnginesModels.ContentExchangerOptions(configDirectoryPath, config?.Exchangers?.ContentExchanger?.MaxConnectionCount ?? 32);
        }

        public static EnginesModels.DeclaredMessageExchangerOptions GenDeclaredMessageExchangerOptions(string configDirectoryPath, Configuration.EnginesConfig config)
        {
            return new EnginesModels.DeclaredMessageExchangerOptions(configDirectoryPath, config?.Exchangers?.DeclaredMessageExchanger?.MaxConnectionCount ?? 32);
        }

        public static EnginesModels.ContentPublisherOptions GenContentPublisherOptions(string configDirectoryPath, Configuration.EnginesConfig config)
        {
            return new EnginesModels.ContentPublisherOptions(configDirectoryPath);
        }

        public static EnginesModels.ContentSubscriberOptions GenContentSubscriberOptions(string configDirectoryPath, Configuration.EnginesConfig config)
        {
            return new EnginesModels.ContentSubscriberOptions(configDirectoryPath);
        }

        public static EnginesModels.DeclaredMessagePublisherOptions GenDeclaredMessagePublisherOptions(string configDirectoryPath, Configuration.EnginesConfig config)
        {
            return new EnginesModels.DeclaredMessagePublisherOptions(configDirectoryPath);
        }

        public static EnginesModels.DeclaredMessageSubscriberOptions GenDeclaredMessageSubscriberOptions(string configDirectoryPath, Configuration.EnginesConfig config)
        {
            return new EnginesModels.DeclaredMessageSubscriberOptions(configDirectoryPath);
        }
    }
}
