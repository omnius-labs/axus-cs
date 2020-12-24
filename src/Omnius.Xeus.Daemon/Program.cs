using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Cocona;
using Omnius.Core;
using Omnius.Core.Network;
using Omnius.Core.Network.Caps;
using Omnius.Core.Network.Connections;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Xeus.Api;
using Omnius.Xeus.Daemon.Configs;
using Omnius.Xeus.Daemon.Internal;
using Omnius.Xeus.Engines.Connectors;
using Omnius.Xeus.Engines.Exchangers;
using Omnius.Xeus.Engines.Mediators;
using Omnius.Xeus.Engines.Storages;

namespace Omnius.Xeus.Daemon
{
    public class Program : CoconaLiteConsoleAppBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            CoconaLiteApp.Run<Program>(args);
        }

        [Command("init")]
        public void Init([Option('c')] string configDirectoryPath = "../config", [Option('s')] string stateDirectoryPath = "../state")
        {
            _logger.Info("init start");

            if (!Directory.Exists(configDirectoryPath))
            {
                Directory.CreateDirectory(configDirectoryPath);
            }

            if (!Directory.Exists(stateDirectoryPath))
            {
                Directory.CreateDirectory(stateDirectoryPath);
            }

            var config = new DaemonConfig()
            {
                ListenAddress = (string?)OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 32321),
                Engines = new EnginesConfig()
                {
                    WorkingDirectory = stateDirectoryPath,
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

            YamlHelper.WriteFile(Path.Combine(configDirectoryPath, "daemon.yaml"), config);

            _logger.Info("init end");
        }

        [Command("run")]
        public async ValueTask RunAsync([Option('c')] string configDirectoryPath = "../config")
        {
            _logger.Info("run start");

            var config = this.LoadConfig(configDirectoryPath);
            if (config.ListenAddress is null)
            {
                throw new Exception("ListenAddress is not found.");
            }

            var listenAddress = new OmniAddress(config.ListenAddress);
            if (!listenAddress.TryParseTcpEndpoint(out var ipAddress, out var port))
            {
                throw new Exception("listenAddress is invalid format.");
            }

            var options = new XeusServiceOptions
            {
                Socks5ProxyClientFactory = Socks5ProxyClient.Factory,
                HttpProxyClientFactory = HttpProxyClient.Factory,
                UpnpClientFactory = UpnpClient.Factory,
                TcpConnectorFactory = TcpConnector.Factory,
                CkadMediatorFactory = CkadMediator.Factory,
                ContentExchangerFactory = ContentExchanger.Factory,
                DeclaredMessageExchangerFactory = DeclaredMessageExchanger.Factory,
                PushContentStorageFactory = PushContentStorage.Factory,
                WantContentStorageFactory = WantContentStorage.Factory,
                PushDeclaredMessageStorageFactory = PushDeclaredMessageStorage.Factory,
                WantDeclaredMessageStorageFactory = WantDeclaredMessageStorage.Factory,
                BytesPool = BytesPool.Shared,
                Config = config.Engines,
            };

            var service = await XeusServiceImpl.CreateAsync(options);

            await this.ListenLoopAsync(service, ipAddress, port);

            _logger.Info("run end");
        }

        private DaemonConfig LoadConfig(string configDirectoryPath)
        {
            var configPath = Path.Combine(configDirectoryPath, "daemon.yaml");
            var config = YamlHelper.ReadFile<DaemonConfig>(configPath);
            return config;
        }

        private async ValueTask ListenLoopAsync(XeusServiceImpl service, IPAddress ipAddress, ushort port)
        {
            TcpListener? tcpListener = null;

            try
            {
                tcpListener = new TcpListener(ipAddress, port);
                tcpListener.Start();
                using var register = this.Context.CancellationToken.Register(() => tcpListener.Stop());

                var dispatcherOptions = new BaseConnectionDispatcherOptions()
                {
                    MaxReceiveBytesPerSeconds = 1024 * 1024 * 32,
                    MaxSendBytesPerSeconds = 1024 * 1024 * 32,
                };
                var dispatcher = new BaseConnectionDispatcher(dispatcherOptions);

                var listenTasks = new List<Task>();

                while (!this.Context.CancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var socket = await tcpListener.AcceptSocketAsync();
                        _logger.Debug("Tcp accepted");

                        var cap = new SocketCap(socket);

                        var connectionOption = new BaseConnectionOptions()
                        {
                            MaxReceiveByteCount = 1024 * 1024 * 32,
                            BytesPool = BytesPool.Shared,
                        };
                        var connection = new BaseConnection(cap, dispatcher, connectionOption);

                        listenTasks.Add(this.EventLoopAsync(service, connection));
                    }
                    catch (SocketException)
                    {
                        break;
                    }
                }

                await Task.WhenAll(listenTasks.ToArray());
            }
            finally
            {
                if (tcpListener is not null)
                {
                    tcpListener.Server.Dispose();
                }
            }
        }

        private async Task EventLoopAsync(XeusServiceImpl service, IConnection connection)
        {
            _logger.Info("event loop start");

            await using var server = new XeusService.Server(service, connection, BytesPool.Shared);
            await server.EventLoop(this.Context.CancellationToken);

            _logger.Info("event loop end");
        }

        private class FilePathExistsAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
            {
                if (value is string path && File.Exists(path))
                {
                    return ValidationResult.Success!;
                }

                return new ValidationResult($"The path '{value}' is not found.");
            }
        }
    }
}
