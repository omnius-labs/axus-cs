using System;
using System.IO;
using System.Linq;
using Omnius.Core.Network;
using Omnius.Xeus.Daemon.Models;

namespace Omnius.Xeus.Daemon.Internal
{
    using Models = Omnius.Xeus.Engines.Models;

    internal class OptionsGenerator
    {
        private static OmniAddress? CreateAddress(string? value)
        {
            if (value is null) return null;
            return new OmniAddress(value);
        }

        public static Models.TcpConnectorOptions GenTcpConnectorOptions(XeusServiceConfig config)
        {
            static Models.TcpConnectingOptions GenTcpConnectingOptions(XeusServiceConfig config)
            {
                return new Models.TcpConnectingOptions(
                    config.Connectors?.TcpConnector?.Connecting?.Enabled ?? false,
                    new Models.TcpProxyOptions(
                        config.Connectors?.TcpConnector?.Connecting?.Proxy?.Type switch
                        {
                            XeusServiceConfig.TcpProxyType.HttpProxy => Models.TcpProxyType.HttpProxy,
                            XeusServiceConfig.TcpProxyType.Socks5Proxy => Models.TcpProxyType.Socks5Proxy,
                            _ => Models.TcpProxyType.Unknown,
                        },
                        CreateAddress(config?.Connectors?.TcpConnector?.Connecting?.Proxy?.Address)
                    )
                );
            }

            static Models.TcpAcceptingOptions GenTcpAcceptingOptions(XeusServiceConfig config)
            {
                return new Models.TcpAcceptingOptions(
                    config?.Connectors?.TcpConnector?.Accepting?.Enabled ?? false,
                    config?.Connectors?.TcpConnector?.Accepting?.ListenAddresses?.Select(n => new OmniAddress(n))?.ToArray() ?? Array.Empty<OmniAddress>(),
                    config?.Connectors?.TcpConnector?.Accepting?.UseUpnp ?? false
                );
            }

            static Models.BandwidthOptions GenBandwidthOptions(XeusServiceConfig config)
            {
                return new Models.BandwidthOptions(
                    config?.Connectors?.TcpConnector?.Bandwidth?.MaxSendBytesPerSeconds ?? 1024 * 1024 * 32,
                    config?.Connectors?.TcpConnector?.Bandwidth?.MaxReceiveBytesPerSeconds ?? 1024 * 1024 * 32
                );
            }

            return new Models.TcpConnectorOptions(
                GenTcpConnectingOptions(config),
                GenTcpAcceptingOptions(config),
                GenBandwidthOptions(config)
            );
        }

        public static Models.CkadMediatorOptions GenCkadMediatorOptions(XeusServiceConfig config)
        {
            if (config.WorkingDirectory is null) throw new NullReferenceException(nameof(config.WorkingDirectory));
            return new Models.CkadMediatorOptions(Path.Combine(config.WorkingDirectory, "node_finder"), 10);
        }

        public static Models.PushContentStorageOptions GenPushContentStorageOptions(XeusServiceConfig config)
        {
            if (config.WorkingDirectory is null) throw new NullReferenceException(nameof(config.WorkingDirectory));
            return new Models.PushContentStorageOptions(Path.Combine(config.WorkingDirectory, "push_content_storage"));
        }

        public static Models.WantContentStorageOptions GenWantContentStorageOptions(XeusServiceConfig config)
        {
            if (config.WorkingDirectory is null) throw new NullReferenceException(nameof(config.WorkingDirectory));
            return new Models.WantContentStorageOptions(Path.Combine(config.WorkingDirectory, "want_content_storage"));
        }

        public static Models.PushDeclaredMessageStorageOptions GenPushDeclaredMessageStorageOptions(XeusServiceConfig config)
        {
            if (config.WorkingDirectory is null) throw new NullReferenceException(nameof(config.WorkingDirectory));
            return new Models.PushDeclaredMessageStorageOptions(Path.Combine(config.WorkingDirectory, "push_declared_message_storage"));
        }

        public static Models.WantDeclaredMessageStorageOptions GenWantDeclaredMessageStorageOptions(XeusServiceConfig config)
        {
            if (config.WorkingDirectory is null) throw new NullReferenceException(nameof(config.WorkingDirectory));
            return new Models.WantDeclaredMessageStorageOptions(Path.Combine(config.WorkingDirectory, "want_declared_message_storage"));
        }

        public static Models.ContentExchangerOptions GenContentExchangerOptions(XeusServiceConfig config)
        {
            if (config.WorkingDirectory is null) throw new NullReferenceException(nameof(config.WorkingDirectory));
            return new Models.ContentExchangerOptions(Path.Combine(config.WorkingDirectory, "content_exchanger"), config?.Exchangers?.ContentExchanger?.MaxConnectionCount ?? 32);
        }

        public static Models.DeclaredMessageExchangerOptions GenDeclaredMessageExchangerOptions(XeusServiceConfig config)
        {
            if (config.WorkingDirectory is null) throw new NullReferenceException(nameof(config.WorkingDirectory));
            return new Models.DeclaredMessageExchangerOptions(Path.Combine(config.WorkingDirectory, "declared_message_exchanger"), config?.Exchangers?.DeclaredMessageExchanger?.MaxConnectionCount ?? 32);
        }
    }
}
