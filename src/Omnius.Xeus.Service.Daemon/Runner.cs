using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Helpers;
using Omnius.Core.Net;
using Omnius.Core.Net.Caps;
using Omnius.Core.Net.Connections.Bridge;
using Omnius.Core.Net.Connections.Multiplexer;
using Omnius.Core.Net.Connections.Multiplexer.V1;
using Omnius.Core.RocketPack.Remoting;
using Omnius.Core.Tasks;
using Omnius.Xeus.Service.Daemon.Configuration;
using Omnius.Xeus.Service.Remoting;

namespace Omnius.Xeus.Service.Daemon
{
    public static partial class Runner
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static async ValueTask EventLoopAsync(string stateDirectoryPath, CancellationToken cancellationToken = default)
        {
            _ = DirectoryHelper.CreateDirectory(stateDirectoryPath);

            var config = await LoadConfigAsync(Path.Combine(stateDirectoryPath, "config.yml"), cancellationToken);
            var service = new XeusService(stateDirectoryPath, config);
            await ListenAsync(service, config, cancellationToken);
        }

        private static async ValueTask<AppConfig> LoadConfigAsync(string configPath, CancellationToken cancellationToken = default)
        {
            var appConfig = await AppConfig.LoadAsync(configPath);
            if (appConfig is not null)
            {
                return appConfig;
            }

            appConfig = new AppConfig()
            {
                Version = 1,
                ListenAddress = OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 32321).ToString(),
                Engines = new EnginesConfig()
                {
                    Bandwidth = new BandwidthConfig()
                    {
                        MaxSendBytesPerSeconds = 1024 * 1024 * 32,
                        MaxReceiveBytesPerSeconds = 1024 * 1024 * 32,
                    },
                    SessionConnector = new SessionConnectorConfig()
                    {
                        TcpConnectors = new[]
                        {
                            new TcpConnectorConfig()
                            {
                                Proxy = null,
                            },
                        },
                    },
                    SessionAccepter = new SessionAccepterConfig()
                    {
                        TcpAccepters = new[]
                        {
                            new TcpAccepterConfig()
                            {
                                UseUpnp = true,
                                ListenAddress = OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 32320).ToString(),
                            },
                        },
                    },
                    NodeFinder = new NodeFinderConfig()
                    {
                        MaxSessionCount = 128,
                    },
                    FileExchanger = new FileExchangerConfig()
                    {
                        MaxSessionCount = 128,
                    },
                    ShoutExchanger = new ShoutExchangerConfig()
                    {
                        MaxSessionCount = 128,
                    },
                },
            };

            await appConfig.SaveAsync(configPath);

            return appConfig;
        }

        private static async ValueTask ListenAsync(XeusService service, AppConfig config, CancellationToken cancellationToken = default)
        {
            if (config.ListenAddress is null)
            {
                throw new Exception($"{nameof(config.ListenAddress)} is not found");
            }

            using var tcpListenerManager = TcpListenerManager.Create(config.ListenAddress, cancellationToken);

            var socket = await tcpListenerManager.AcceptSocketAsync();
            await ServiceEventLoopAsync(service, socket, cancellationToken);
        }

        private static async ValueTask ServiceEventLoopAsync(XeusService service, Socket socket, CancellationToken cancellationToken = default)
        {
            using var socketCap = new SocketCap(socket);

            var bytesPool = BytesPool.Shared;
            await using var batchActionDispatcher = new BatchActionDispatcher(TimeSpan.FromMilliseconds(10));

            var bridgeConnectionOptions = new BridgeConnectionOptions(int.MaxValue);
            await using var bridgeConnection = new BridgeConnection(socketCap, null, null, batchActionDispatcher, bytesPool, bridgeConnectionOptions);

            var multiplexerOption = new OmniConnectionMultiplexerOptions(OmniConnectionMultiplexerType.Accepted, TimeSpan.FromMinutes(1), 3, 1024 * 1024 * 4, 3);
            await using var multiplexer = OmniConnectionMultiplexer.CreateV1(bridgeConnection, batchActionDispatcher, bytesPool, multiplexerOption);

            await multiplexer.HandshakeAsync(cancellationToken);

            var errorMessageFactory = new DefaultErrorMessageFactory();
            var listenerFactory = new RocketRemotingListenerFactory<DefaultErrorMessage>(multiplexer, errorMessageFactory, bytesPool);

            try
            {
                _logger.Info("event loop start");

                var server = new XeusServiceRemoting.Server<DefaultErrorMessage>(service, listenerFactory, bytesPool);

                var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                using var onClose = bridgeConnection.Events.OnClosed.Subscribe(() => linkedCancellationTokenSource.Cancel());

                await server.EventLoopAsync(linkedCancellationTokenSource.Token);
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
