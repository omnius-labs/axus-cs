using System;
using System.IO;
using System.Linq;
using Omnius.Core.Network;
using EnginesModels = Omnius.Xeus.Engines.Models;
using ResourcesModels = Omnius.Xeus.Daemon.Resources.Models;

namespace Omnius.Xeus.Daemon.Internal
{
    internal class OptionsGenerator
    {
        private static OmniAddress? CreateAddress(string? value)
        {
            if (value is null) return null;

            return new OmniAddress(value);
        }

        public static EnginesModels.TcpConnectorOptions GenTcpConnectorOptions(ResourcesModels.EnginesConfig config)
        {
            static EnginesModels.TcpConnectingOptions GenTcpConnectingOptions(ResourcesModels.EnginesConfig config)
            {
                return new EnginesModels.TcpConnectingOptions(
                    config.Connectors?.TcpConnector?.Connecting?.Enabled ?? false,
                    new EnginesModels.TcpProxyOptions(
                        config.Connectors?.TcpConnector?.Connecting?.Proxy?.Type switch
                        {
                            ResourcesModels.TcpProxyType.HttpProxy => EnginesModels.TcpProxyType.HttpProxy,
                            ResourcesModels.TcpProxyType.Socks5Proxy => EnginesModels.TcpProxyType.Socks5Proxy,
                            _ => EnginesModels.TcpProxyType.Unknown,
                        },
                        CreateAddress(config?.Connectors?.TcpConnector?.Connecting?.Proxy?.Address)));
            }

            static EnginesModels.TcpAcceptingOptions GenTcpAcceptingOptions(ResourcesModels.EnginesConfig config)
            {
                return new EnginesModels.TcpAcceptingOptions(
                    config?.Connectors?.TcpConnector?.Accepting?.Enabled ?? false,
                    config?.Connectors?.TcpConnector?.Accepting?.ListenAddresses?.Select(n => new OmniAddress(n))?.ToArray() ?? Array.Empty<OmniAddress>(),
                    config?.Connectors?.TcpConnector?.Accepting?.UseUpnp ?? false);
            }

            static EnginesModels.BandwidthOptions GenBandwidthOptions(ResourcesModels.EnginesConfig config)
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

        public static EnginesModels.CkadMediatorOptions GenCkadMediatorOptions(ResourcesModels.EnginesConfig config)
        {
            if (config.WorkingDirectory is null) throw new NullReferenceException(nameof(config.WorkingDirectory));

            return new EnginesModels.CkadMediatorOptions(Path.Combine(config.WorkingDirectory, "ckad_mediator"), 10);
        }

        public static EnginesModels.ContentExchangerOptions GenContentExchangerOptions(ResourcesModels.EnginesConfig config)
        {
            if (config.WorkingDirectory is null) throw new NullReferenceException(nameof(config.WorkingDirectory));

            return new EnginesModels.ContentExchangerOptions(Path.Combine(config.WorkingDirectory, "content_exchanger"), config?.Exchangers?.ContentExchanger?.MaxConnectionCount ?? 32);
        }

        public static EnginesModels.DeclaredMessageExchangerOptions GenDeclaredMessageExchangerOptions(ResourcesModels.EnginesConfig config)
        {
            if (config.WorkingDirectory is null) throw new NullReferenceException(nameof(config.WorkingDirectory));

            return new EnginesModels.DeclaredMessageExchangerOptions(Path.Combine(config.WorkingDirectory, "declared_message_exchanger"), config?.Exchangers?.DeclaredMessageExchanger?.MaxConnectionCount ?? 32);
        }

        public static EnginesModels.ContentPublisherOptions GenContentPublisherOptions(ResourcesModels.EnginesConfig config)
        {
            if (config.WorkingDirectory is null) throw new NullReferenceException(nameof(config.WorkingDirectory));

            return new EnginesModels.ContentPublisherOptions(Path.Combine(config.WorkingDirectory, "content_publisher"));
        }

        public static EnginesModels.ContentSubscriberOptions GenContentSubscriberOptions(ResourcesModels.EnginesConfig config)
        {
            if (config.WorkingDirectory is null) throw new NullReferenceException(nameof(config.WorkingDirectory));

            return new EnginesModels.ContentSubscriberOptions(Path.Combine(config.WorkingDirectory, "content_subscriber"));
        }

        public static EnginesModels.DeclaredMessagePublisherOptions GenDeclaredMessagePublisherOptions(ResourcesModels.EnginesConfig config)
        {
            if (config.WorkingDirectory is null) throw new NullReferenceException(nameof(config.WorkingDirectory));

            return new EnginesModels.DeclaredMessagePublisherOptions(Path.Combine(config.WorkingDirectory, "declared_message_publisher"));
        }

        public static EnginesModels.DeclaredMessageSubscriberOptions GenDeclaredMessageSubscriberOptions(ResourcesModels.EnginesConfig config)
        {
            if (config.WorkingDirectory is null) throw new NullReferenceException(nameof(config.WorkingDirectory));

            return new EnginesModels.DeclaredMessageSubscriberOptions(Path.Combine(config.WorkingDirectory, "declared_message_subscriber"));
        }
    }
}
