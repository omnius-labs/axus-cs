using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Cocona;
using Omnius.Core;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Xeus.Daemon.Internal;
using Omnius.Xeus.Daemon.Models;
using Omnius.Core.Network;
using Omnius.Core.Network.Connections;
using Omnius.Core.Network.Caps;
using System.Collections.Generic;
using System;
using Omnius.Xeus.Api;

namespace Omnius.Xeus.Daemon
{
    class Program : CoconaLiteConsoleAppBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            CoconaLiteApp.Run<Program>(args);
        }

        [Command("init")]
        public void Init([Option('c')] string configDirectoryPath, [Option('s')] string stateDirectoryPath)
        {
            if (!Directory.Exists(configDirectoryPath)) Directory.CreateDirectory(configDirectoryPath);
            if (!Directory.Exists(stateDirectoryPath)) Directory.CreateDirectory(stateDirectoryPath);

            var xeusServiceConfig = new XeusServiceConfig()
            {
                WorkingDirectory = stateDirectoryPath,
            };

            var daemonConfig = new DaemonConfig()
            {
                ListenAddress = (string?)OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 32321),
            };

            YamlHelper.WriteFile(Path.Combine(configDirectoryPath, "xeus-service.yaml"), xeusServiceConfig);
            YamlHelper.WriteFile(Path.Combine(configDirectoryPath, "daemon.yaml"), daemonConfig);
        }

        [Command("run")]
        public async ValueTask RunAsync([Option('c')] string configDirectoryPath, [Option('s')] string stateDirectoryPath)
        {
            var xeusServiceConfigPath = Path.Combine(configDirectoryPath, "xeus-service.yaml");
            var daemonConfigPath = Path.Combine(configDirectoryPath, "daemon.yaml");

            var xeusServiceConfig = YamlHelper.ReadFile<XeusServiceConfig>(xeusServiceConfigPath);
            var daemonConfig = YamlHelper.ReadFile<DaemonConfig>(daemonConfigPath);

            var options = new XeusServiceOptions
            {
                Socks5ProxyClientFactory = Socks5ProxyClient.Factory,
                HttpProxyClientFactory = HttpProxyClient.Factory,
                UpnpClientFactory = UpnpClient.Factory,
                TcpConnectorFactory = TcpConnector.Factory,
                CkadMediatorFactory = CkadMediator.Factory,
                ContentExchangerFactory = ContentExchanger.Factory,
                DeclaredMessageExchangerFactory = DeclaredMessageExchanger.Factory,
                BytesPool = BytesPool.Shared,
                Config = xeusServiceConfig,
            };

            var service = await XeusService.CreateAsync(options);

            var listenAddress = (OmniAddress?)daemonConfig?.ListenAddress;
            if (listenAddress is null || !listenAddress.TryParseTcpEndpoint(out var ipAddress, out var port))
            {
                _logger.Error($"load is failed of  ListenAddress. \"{daemonConfigPath}\"");
                return;
            }

            await this.ListenAsync(service, ipAddress, port);
        }

        private async ValueTask ListenAsync(XeusService service, IPAddress ipAddress, ushort port)
        {
            TcpListener? tcpListener = null;

            try
            {
                tcpListener = new TcpListener(ipAddress, port);
                tcpListener.Start();

                using (this.Context.CancellationToken.Register(() => tcpListener.Stop()))
                {
                    var dispatcherOptions = new BaseConnectionDispatcherOptions()
                    {
                        MaxReceiveBytesPerSeconds = 1024 * 1024 * 32,
                        MaxSendBytesPerSeconds = 1024 * 1024 * 32,
                    };
                    var dispatcher = new BaseConnectionDispatcher(dispatcherOptions);

                    var receiverTasks = new List<Task>();

                    while (!this.Context.CancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var socket = await tcpListener.AcceptSocketAsync();
                            var cap = new SocketCap(socket);

                            var connectionOption = new BaseConnectionOptions()
                            {
                                MaxReceiveByteCount = 1024 * 1024 * 32,
                                BytesPool = BytesPool.Shared,
                            };
                            var connection = new BaseConnection(cap, dispatcher, connectionOption);

                            var server = new XeusServiceServer(service, connection, BytesPool.Shared);
                            receiverTasks.Add(server.EventLoop().ContinueWith(async (_) => await server.DisposeAsync()));
                        }
                        catch (SocketException)
                        {
                            break;
                        }
                    }

                    await Task.WhenAll(receiverTasks.ToArray());
                }
            }
            finally
            {
                if (tcpListener is not null) tcpListener.Server.Dispose();
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
