using System;
using System.Linq;
using Omnius.Core.Network;
using Omnius.Xeus.Deamon.Models;

namespace Omnius.Xeus.Deamon.Internal
{
    using Models = Omnius.Xeus.Components.Models;

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
    }
}
