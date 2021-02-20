using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Helpers;
using Omnius.Core.Network;
using Omnius.Core.Network.Caps;
using Omnius.Core.Network.Connections;
using Omnius.Xeus.Api;
using Omnius.Xeus.Daemon.Resources.Models;

namespace Omnius.Xeus.Daemon
{
    public class Daemon : DisposableBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public Daemon()
        {
        }

        protected override void OnDispose(bool disposing)
        {
        }

        public async ValueTask RunAsync(string stateDirectoryPath, CancellationToken cancellationToken = default)
        {
            DirectoryHelper.CreateDirectory(stateDirectoryPath);

            var config = await this.CreateConfigAsync(Path.Combine(stateDirectoryPath, "config.yml"), stateDirectoryPath, cancellationToken);

            if (config.Engines is null) throw new Exception($"{nameof(config.Engines)} is not found");
            if (config.ListenAddress is null) throw new Exception($"{nameof(config.ListenAddress)} is not found");

            await using var service = await XeusServiceImpl.CreateAsync(config.Engines, cancellationToken);
            await this.ListenAsync(service, config.ListenAddress, cancellationToken);
        }

        private async ValueTask<Config> CreateConfigAsync(string configPath, string workingDirPath, CancellationToken cancellationToken = default)
        {
            var config = await Config.LoadAsync(configPath);
            if (config is not null) return config;

            var enginesWorkingDirPath = Path.Combine(workingDirPath, "omnius.xeus.engines");
            DirectoryHelper.CreateDirectory(enginesWorkingDirPath);

            config = new Config()
            {
                Version = 1,
                ListenAddress = (string?)OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 32321),
                Engines = new EnginesConfig()
                {
                    WorkingDirectory = enginesWorkingDirPath,
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

            return config;
        }

        private async ValueTask ListenAsync(XeusServiceImpl service, string listenAddress, CancellationToken cancellationToken = default)
        {
            var listenOmniAddress = new OmniAddress(listenAddress);
            if (!listenOmniAddress.TryParseTcpEndpoint(out var ipAddress, out var port)) throw new Exception("listenAddress is invalid format.");
            TcpListener? tcpListener = null;

            try
            {
                _logger.Info("listen start");

                tcpListener = new TcpListener(ipAddress, port);
                tcpListener.Start();

                using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                linkedTokenSource.CancelAfter(TimeSpan.FromMinutes(5));
                using var register = linkedTokenSource.Token.Register(() => tcpListener.Stop());

                var socket = await tcpListener.AcceptSocketAsync();

                var cap = new SocketCap(socket);

                var dispatcherOptions = new BaseConnectionDispatcherOptions()
                {
                    MaxReceiveBytesPerSeconds = 1024 * 1024 * 32,
                    MaxSendBytesPerSeconds = 1024 * 1024 * 32,
                };
                var dispatcher = new BaseConnectionDispatcher(dispatcherOptions);

                var connectionOption = new BaseConnectionOptions()
                {
                    MaxReceiveByteCount = 1024 * 1024 * 32,
                    BytesPool = BytesPool.Shared,
                };
                var connection = new BaseConnection(cap, dispatcher, connectionOption);

                _logger.Info("event loop start");

                try
                {
                    await using var server = new XeusService.Server(service, connection, BytesPool.Shared);
                    await server.EventLoopAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.Debug(e);
                }

                _logger.Info("event loop end");
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
