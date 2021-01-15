using System;
using System.IO;
using System.Linq;
using Omnius.Core.Network;

namespace Omnius.Xeus.Daemon.Internal
{
    using Configs = Omnius.Xeus.Daemon.Configs;
    using Models = Omnius.Xeus.Engines.Models;

    internal class OptionsGenerator
    {
        private static OmniAddress? CreateAddress(string? value)
        {
            if (value is null)
            {
                return null;
            }

            return new OmniAddress(value);
        }

        public static Models.TcpConnectorOptions GenTcpConnectorOptions(Configs.EnginesConfig config)
        {
            static Models.TcpConnectingOptions GenTcpConnectingOptions(Configs.EnginesConfig config)
            {
                return new Models.TcpConnectingOptions(
                    config.Connectors?.TcpConnector?.Connecting?.Enabled ?? false,
                    new Models.TcpProxyOptions(
                        config.Connectors?.TcpConnector?.Connecting?.Proxy?.Type switch
                        {
                            Configs.TcpProxyType.HttpProxy => Models.TcpProxyType.HttpProxy,
                            Configs.TcpProxyType.Socks5Proxy => Models.TcpProxyType.Socks5Proxy,
                            _ => Models.TcpProxyType.Unknown,
                        },
                        CreateAddress(config?.Connectors?.TcpConnector?.Connecting?.Proxy?.Address)));
            }

            static Models.TcpAcceptingOptions GenTcpAcceptingOptions(Configs.EnginesConfig config)
            {
                return new Models.TcpAcceptingOptions(
                    config?.Connectors?.TcpConnector?.Accepting?.Enabled ?? false,
                    config?.Connectors?.TcpConnector?.Accepting?.ListenAddresses?.Select(n => new OmniAddress(n))?.ToArray() ?? Array.Empty<OmniAddress>(),
                    config?.Connectors?.TcpConnector?.Accepting?.UseUpnp ?? false);
            }

            static Models.BandwidthOptions GenBandwidthOptions(Configs.EnginesConfig config)
            {
                return new Models.BandwidthOptions(
                    config?.Connectors?.TcpConnector?.Bandwidth?.MaxSendBytesPerSeconds ?? 1024 * 1024 * 32,
                    config?.Connectors?.TcpConnector?.Bandwidth?.MaxReceiveBytesPerSeconds ?? 1024 * 1024 * 32);
            }

            return new Models.TcpConnectorOptions(
                GenTcpConnectingOptions(config),
                GenTcpAcceptingOptions(config),
                GenBandwidthOptions(config));
        }

        public static Models.CkadMediatorOptions GenCkadMediatorOptions(Configs.EnginesConfig config)
        {
            if (config.WorkingDirectory is null)
            {
                throw new NullReferenceException(nameof(config.WorkingDirectory));
            }

            return new Models.CkadMediatorOptions(Path.Combine(config.WorkingDirectory, "ckad_mediator"), 10);
        }

        public static Models.ContentExchangerOptions GenContentExchangerOptions(Configs.EnginesConfig config)
        {
            if (config.WorkingDirectory is null)
            {
                throw new NullReferenceException(nameof(config.WorkingDirectory));
            }

            return new Models.ContentExchangerOptions(Path.Combine(config.WorkingDirectory, "content_exchanger"), config?.Exchangers?.ContentExchanger?.MaxConnectionCount ?? 32);
        }

        public static Models.DeclaredMessageExchangerOptions GenDeclaredMessageExchangerOptions(Configs.EnginesConfig config)
        {
            if (config.WorkingDirectory is null)
            {
                throw new NullReferenceException(nameof(config.WorkingDirectory));
            }

            return new Models.DeclaredMessageExchangerOptions(Path.Combine(config.WorkingDirectory, "declared_message_exchanger"), config?.Exchangers?.DeclaredMessageExchanger?.MaxConnectionCount ?? 32);
        }

        public static Models.ContentPublisherOptions GenContentPublisherOptions(Configs.EnginesConfig config)
        {
            if (config.WorkingDirectory is null)
            {
                throw new NullReferenceException(nameof(config.WorkingDirectory));
            }

            return new Models.ContentPublisherOptions(Path.Combine(config.WorkingDirectory, "content_publisher"));
        }

        public static Models.ContentSubscriberOptions GenContentSubscriberOptions(Configs.EnginesConfig config)
        {
            if (config.WorkingDirectory is null)
            {
                throw new NullReferenceException(nameof(config.WorkingDirectory));
            }

            return new Models.ContentSubscriberOptions(Path.Combine(config.WorkingDirectory, "content_subscriber"));
        }

        public static Models.DeclaredMessagePublisherOptions GenDeclaredMessagePublisherOptions(Configs.EnginesConfig config)
        {
            if (config.WorkingDirectory is null)
            {
                throw new NullReferenceException(nameof(config.WorkingDirectory));
            }

            return new Models.DeclaredMessagePublisherOptions(Path.Combine(config.WorkingDirectory, "declared_message_publisher"));
        }

        public static Models.DeclaredMessageSubscriberOptions GenDeclaredMessageSubscriberOptions(Configs.EnginesConfig config)
        {
            if (config.WorkingDirectory is null)
            {
                throw new NullReferenceException(nameof(config.WorkingDirectory));
            }

            return new Models.DeclaredMessageSubscriberOptions(Path.Combine(config.WorkingDirectory, "declared_message_subscriber"));
        }
    }
}
