using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog.Fluent;
using Omnius.Core;
using Omnius.Core.Helpers;
using Omnius.Core.Network;
using Omnius.Core.Network.Caps;
using Omnius.Core.Network.Connections;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Core.Storages;
using Omnius.Xeus.Api;
using Omnius.Xeus.Daemon.Internal;
using Omnius.Xeus.Daemon.Resources.Models;
using Omnius.Xeus.Engines.Connectors;
using Omnius.Xeus.Engines.Exchangers;
using Omnius.Xeus.Engines.Mediators;
using Omnius.Xeus.Engines.Storages;

namespace Omnius.Xeus.Daemon
{
    public class Runner : DisposableBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public Runner()
        {
        }

        protected override void OnDispose(bool disposing)
        {
        }

        public async ValueTask RunAsync(string stateDirectoryPath, CancellationToken cancellationToken = default)
        {
            DirectoryHelper.CreateDirectory(stateDirectoryPath);

            var config = await this.LoadConfigAsync(Path.Combine(stateDirectoryPath, "config.yml"), cancellationToken);

            if (config.Engines is null) throw new Exception($"{nameof(config.Engines)} is not found");
            if (config.ListenAddress is null) throw new Exception($"{nameof(config.ListenAddress)} is not found");

            var options = new XeusServiceOptions()
            {
                BytesPool = BytesPool.Shared,
                Socks5ProxyClientFactory = Socks5ProxyClient.Factory,
                HttpProxyClientFactory = HttpProxyClient.Factory,
                UpnpClientFactory = UpnpClient.Factory,
                BytesStorageFactory = LiteDatabaseBytesStorage.Factory,
                TcpConnectorFactory = TcpConnector.Factory,
                CkadMediatorFactory = CkadMediator.Factory,
                ContentExchangerFactory = ContentExchanger.Factory,
                DeclaredMessageExchangerFactory = DeclaredMessageExchanger.Factory,
                ContentPublisherFactory = ContentPublisher.Factory,
                ContentSubscriberFactory = ContentSubscriber.Factory,
                DeclaredMessagePublisherFactory = DeclaredMessagePublisher.Factory,
                DeclaredMessageSubscriberFactory = DeclaredMessageSubscriber.Factory,
                TcpConnectorOptions = OptionsGenerator.GenTcpConnectorOptions(config.Engines),
                CkadMediatorOptions = OptionsGenerator.GenCkadMediatorOptions(Path.Combine(stateDirectoryPath, "omnius.xeus.engines", "ckad_mediator"), config.Engines),
                ContentPublisherOptions = OptionsGenerator.GenContentPublisherOptions(Path.Combine(stateDirectoryPath, "omnius.xeus.engines", "content_publisher"), config.Engines),
                ContentSubscriberOptions = OptionsGenerator.GenContentSubscriberOptions(Path.Combine(stateDirectoryPath, "omnius.xeus.engines", "content_subscriber"), config.Engines),
                DeclaredMessagePublisherOptions = OptionsGenerator.GenDeclaredMessagePublisherOptions(Path.Combine(stateDirectoryPath, "omnius.xeus.engines", "declared_message_publisher"), config.Engines),
                DeclaredMessageSubscriberOptions = OptionsGenerator.GenDeclaredMessageSubscriberOptions(Path.Combine(stateDirectoryPath, "omnius.xeus.engines", "declared_message_subscriber"), config.Engines),
                ContentExchangerOptions = OptionsGenerator.GenContentExchangerOptions(Path.Combine(stateDirectoryPath, "omnius.xeus.engines", "content_exchanger"), config.Engines),
                DeclaredMessageExchangerOptions = OptionsGenerator.GenDeclaredMessageExchangerOptions(Path.Combine(stateDirectoryPath, "omnius.xeus.engines", "declared_message_exchanger"), config.Engines),
            };

            await using var service = await XeusServiceImpl.CreateAsync(options, cancellationToken);
            await this.ListenAsync(service, config.ListenAddress, cancellationToken);
        }

        private async ValueTask<Config> LoadConfigAsync(string configPath, CancellationToken cancellationToken = default)
        {
            var config = await Config.LoadAsync(configPath);
            if (config is not null) return config;

            config = new Config()
            {
                Version = 1,
                ListenAddress = OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 32321).ToString(),
                Engines = new EnginesConfig()
                {
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

            await config.SaveAsync(configPath);

            return config;
        }

        private async ValueTask ListenAsync(XeusServiceImpl service, string listenAddress, CancellationToken cancellationToken = default)
        {
            var listenOmniAddress = new OmniAddress(listenAddress);
            if (!listenOmniAddress.TryGetTcpEndpoint(out var ipAddress, out var port)) throw new Exception("listenAddress is invalid format.");
            TcpListener? tcpListener = null;

            try
            {
                _logger.Info()
                    .Message("listen start")
                    .Properties(new Dictionary<string, object?>
                    {
                        { "ip_address", ipAddress?.ToString() },
                        { "port", port.ToString() },
                    })
                    .Write();

                tcpListener = new TcpListener(ipAddress!, port);
                tcpListener.Start();
                using var canceller = cancellationToken.Register(() => tcpListener.Stop());

                while (!cancellationToken.IsCancellationRequested)
                {
                    var socket = await tcpListener.AcceptSocketAsync();

                    var cap = new SocketCap(socket);

                    var dispatcherOptions = new BaseConnectionDispatcherOptions()
                    {
                        MaxReceiveBytesPerSeconds = 1024 * 1024 * 32,
                        MaxSendBytesPerSeconds = 1024 * 1024 * 32,
                    };
                    await using var dispatcher = new BaseConnectionDispatcher(dispatcherOptions);

                    var connectionOption = new BaseConnectionOptions()
                    {
                        MaxReceiveByteCount = 1024 * 1024 * 32,
                        BytesPool = BytesPool.Shared,
                    };
                    using var connection = new BaseConnection(cap, dispatcher, connectionOption);

                    try
                    {
                        _logger.Info("event loop start");

                        await using var server = new XeusService.Server(service, connection, BytesPool.Shared);
                        await server.EventLoopAsync(cancellationToken);
                    }
                    catch (Exception e)
                    {
                        _logger.Debug(e);
                    }
                    finally
                    {
                        _logger.Info("event loop end");
                    }
                }
            }
            finally
            {
                _logger.Info("listen end");

                if (tcpListener is not null)
                {
                    tcpListener.Server.Dispose();
                }
            }
        }
    }
}
