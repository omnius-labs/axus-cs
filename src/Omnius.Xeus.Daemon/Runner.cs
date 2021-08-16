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
using Omnius.Core.Net;
using Omnius.Core.Net.Caps;
using Omnius.Core.Net.Connections.Bridge;
using Omnius.Core.Net.Connections.Multiplexer;
using Omnius.Core.Net.Connections.Multiplexer.V1;
using Omnius.Core.Net.Proxies;
using Omnius.Core.Net.Upnp;
using Omnius.Core.RocketPack.Remoting;
using Omnius.Core.Storages;
using Omnius.Core.Tasks;
using Omnius.Xeus.Daemon.Configuration;
using Omnius.Xeus.Daemon.Internal;
using Omnius.Xeus.Engines.Connectors;
using Omnius.Xeus.Engines.Exchangers;
using Omnius.Xeus.Engines.Mediators;
using Omnius.Xeus.Engines;

namespace Omnius.Xeus.Daemon
{
    public static class Runner
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static async ValueTask EventLoopAsync(string stateDirectoryPath, CancellationToken cancellationToken = default)
        {
            DirectoryHelper.CreateDirectory(stateDirectoryPath);

            var config = await LoadConfigAsync(Path.Combine(stateDirectoryPath, "config.yml"), cancellationToken);
            var service = await CreateXeusService(stateDirectoryPath, config, cancellationToken);
            await ListenEventLoopAsync(service, config, cancellationToken);
        }

        private static async ValueTask<AppConfig> LoadConfigAsync(string configPath, CancellationToken cancellationToken = default)
        {
            var config = await AppConfig.LoadAsync(configPath);
            if (config is not null) return config;

            config = new AppConfig()
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

        private static async Task<XeusService> CreateXeusService(string stateDirectoryPath, AppConfig config, CancellationToken cancellationToken)
        {
            if (config.Engines is null) throw new Exception($"{nameof(config.Engines)} is not found");

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
                PublishedFileStorageFactory = PublishedFileStorage.Factory,
                SubscribedFileStorageFactory = SubscribedFileStorage.Factory,
                PublishedDeclaredMessageFactory = PublishedDeclaredMessage.Factory,
                SubscribedDeclaredMessageFactory = SubscribedDeclaredMessage.Factory,
                TcpConnectionFactoryOptions = OptionsGenerator.GenTcpConnectionFactoryOptions(config.Engines),
                CkadMediatorOptions = OptionsGenerator.GenCkadMediatorOptions(Path.Combine(stateDirectoryPath, "omnius.xeus.engines", "ckad_mediator"), config.Engines),
                PublishedFileStorageOptions = OptionsGenerator.GenPublishedFileStorageOptions(Path.Combine(stateDirectoryPath, "omnius.xeus.engines", "content_publisher"), config.Engines),
                SubscribedFileStorageOptions = OptionsGenerator.GenSubscribedFileStorageOptions(Path.Combine(stateDirectoryPath, "omnius.xeus.engines", "content_subscriber"), config.Engines),
                PublishedDeclaredMessageOptions = OptionsGenerator.GenPublishedDeclaredMessageOptions(Path.Combine(stateDirectoryPath, "omnius.xeus.engines", "declared_message_publisher"), config.Engines),
                SubscribedDeclaredMessageOptions = OptionsGenerator.GenSubscribedDeclaredMessageOptions(Path.Combine(stateDirectoryPath, "omnius.xeus.engines", "declared_message_subscriber"), config.Engines),
                ContentExchangerOptions = OptionsGenerator.GenContentExchangerOptions(Path.Combine(stateDirectoryPath, "omnius.xeus.engines", "content_exchanger"), config.Engines),
                DeclaredMessageExchangerOptions = OptionsGenerator.GenDeclaredMessageExchangerOptions(Path.Combine(stateDirectoryPath, "omnius.xeus.engines", "declared_message_exchanger"), config.Engines),
            };

            var service = await XeusService.CreateAsync(options, cancellationToken);
            return service;
        }

        private static async ValueTask ListenEventLoopAsync(XeusService service, AppConfig config, CancellationToken cancellationToken = default)
        {
            if (config.ListenAddress is null) throw new Exception($"{nameof(config.ListenAddress)} is not found");

            using var tcpListenerManager = TcpListenerManager.Create(config.ListenAddress, cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                var socket = await tcpListenerManager.AcceptSocketAsync();
                await ServiceEventLoopAsync(service, socket, cancellationToken);
            }
        }

        private sealed class TcpListenerManager : DisposableBase
        {
            private readonly TcpListener _tcpListener;
            private readonly CancellationTokenRegistration _registration;

            public static TcpListenerManager Create(string listenAddress, CancellationToken cancellationToken = default)
            {
                return new TcpListenerManager(listenAddress, cancellationToken);
            }

            private TcpListenerManager(string listenAddress, CancellationToken cancellationToken = default)
            {
                var listenOmniAddress = new OmniAddress(listenAddress);
                if (!listenOmniAddress.TryGetTcpEndpoint(out var ipAddress, out var port)) throw new Exception("listenAddress is invalid format.");

                _tcpListener = new TcpListener(ipAddress!, port);
                _tcpListener.Start();
                _registration = cancellationToken.Register(() => _tcpListener.Stop());
            }

            public async ValueTask<Socket> AcceptSocketAsync()
            {
                return await _tcpListener.AcceptSocketAsync();
            }

            protected override void OnDispose(bool disposing)
            {
                _logger.Info("listen stop");

                _registration.Dispose();

                if (_tcpListener is not null)
                {
                    _tcpListener.Server.Dispose();
                }
            }
        }

        private static async ValueTask ServiceEventLoopAsync(XeusService service, Socket socket, CancellationToken cancellationToken = default)
        {
            using var socketCap = new SocketCap(socket);

            var bytesPool = BytesPool.Shared;
            await using var batchActionDispatcher = new BatchActionDispatcher(TimeSpan.FromMilliseconds(10));

            var bridgeConnectionOptions = new BridgeConnectionOptions
            {
                MaxReceiveByteCount = int.MaxValue,
                SenderBandwidthLimiter = null,
                ReceiverBandwidthLimiter = null,
                BatchActionDispatcher = batchActionDispatcher,
                BytesPool = bytesPool,
            };
            await using var bridgeConnection = new BridgeConnection(socketCap, bridgeConnectionOptions);

            var multiplexerOption = new OmniConnectionMultiplexerOptions
            {
                Type = OmniConnectionMultiplexerType.Accepted,
            };
            await using var multiplexer = new OmniConnectionMultiplexer(bridgeConnection, multiplexerOption);

            var errorMessageFactory = new DefaultErrorMessageFactory();
            var listenerFactory = new RocketRemotingListenerFactory<DefaultErrorMessage>(multiplexer, errorMessageFactory, bytesPool);

            try
            {
                _logger.Info("event loop start");

                var server = new XeusServiceRemoting.Server<DefaultErrorMessage>(service, listenerFactory, bytesPool);
                await server.EventLoopAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.Debug(e);
            }
            finally
            {
                _logger.Info("event loop stop");
            }
        }
    }
}
