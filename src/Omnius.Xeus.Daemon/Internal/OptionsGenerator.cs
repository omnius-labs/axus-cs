using System;
using System.Linq;
using Omnius.Core.Net;
using EnginesModels = Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Daemon.Internal
{
    internal class OptionsGenerator
    {
        public static EnginesModels.TcpConnectionFactoryOptions GenTcpConnectionFactoryOptions(Configuration.EnginesConfig config)
        {
            return new EnginesModels.TcpConnectionFactoryOptions(
                GenTcpConnectingOptions(config),
                GenTcpAcceptingOptions(config),
                GenBandwidthOptions(config));
        }

        private static EnginesModels.TcpConnectingOptions GenTcpConnectingOptions(Configuration.EnginesConfig config)
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

        private static EnginesModels.TcpAcceptingOptions GenTcpAcceptingOptions(Configuration.EnginesConfig config)
        {
            return new EnginesModels.TcpAcceptingOptions(
                config?.Connectors?.TcpConnector?.Accepting?.Enabled ?? false,
                config?.Connectors?.TcpConnector?.Accepting?.ListenAddresses?.Select(n => new OmniAddress(n))?.ToArray() ?? Array.Empty<OmniAddress>(),
                config?.Connectors?.TcpConnector?.Accepting?.UseUpnp ?? false);
        }

        private static EnginesModels.BandwidthOptions GenBandwidthOptions(Configuration.EnginesConfig config)
        {
            return new EnginesModels.BandwidthOptions(
                config?.Connectors?.TcpConnector?.Bandwidth?.MaxSendBytesPerSeconds ?? 1024 * 1024 * 32,
                config?.Connectors?.TcpConnector?.Bandwidth?.MaxReceiveBytesPerSeconds ?? 1024 * 1024 * 32);
        }

        private static OmniAddress? CreateAddress(string? value)
        {
            if (value is null) return null;

            return new OmniAddress(value);
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

        public static EnginesModels.PublishedFileStorageOptions GenPublishedFileStorageOptions(string configDirectoryPath, Configuration.EnginesConfig config)
        {
            return new EnginesModels.PublishedFileStorageOptions(configDirectoryPath);
        }

        public static EnginesModels.SubscribedFileStorageOptions GenSubscribedFileStorageOptions(string configDirectoryPath, Configuration.EnginesConfig config)
        {
            return new EnginesModels.SubscribedFileStorageOptions(configDirectoryPath);
        }

        public static EnginesModels.PublishedDeclaredMessageOptions GenPublishedDeclaredMessageOptions(string configDirectoryPath, Configuration.EnginesConfig config)
        {
            return new EnginesModels.PublishedDeclaredMessageOptions(configDirectoryPath);
        }

        public static EnginesModels.SubscribedDeclaredMessageOptions GenSubscribedDeclaredMessageOptions(string configDirectoryPath, Configuration.EnginesConfig config)
        {
            return new EnginesModels.SubscribedDeclaredMessageOptions(configDirectoryPath);
        }
    }
}
