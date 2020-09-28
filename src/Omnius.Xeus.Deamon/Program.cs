using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Cocona;
using Omnius.Core;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Xeus.Components.Connectors;
using Omnius.Xeus.Components.Engines;
using Omnius.Xeus.Deamon.Internal;
using Omnius.Xeus.Deamon.Models;
using Omnius.Xeus.Service;
using Omnius.Core.Network;
using Omnius.Core.Network.Connections;
using Omnius.Core.Network.Caps;
using System.Collections.Generic;
using System;

namespace Omnius.Xeus.Deamon
{
    class Program : CoconaLiteConsoleAppBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            CoconaLiteApp.Run<Program>(args);
        }

        public void Init([Option('d')] string? directory = null)
        {
            directory ??= Directory.GetCurrentDirectory();
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            var configDirectoryPath = Path.Combine(directory, ".config");
            if (!Directory.Exists(configDirectoryPath)) Directory.CreateDirectory(configDirectoryPath);

            var statusDirectoryPath = Path.Combine(configDirectoryPath, "status");
            if (!Directory.Exists(statusDirectoryPath)) Directory.CreateDirectory(statusDirectoryPath);

            var xeusServiceConfig = new XeusServiceConfig()
            {
                WorkingDirectory = statusDirectoryPath,
            };

            var deamonConfig = new DeamonConfig()
            {
                ListenAddress = (string?)OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 32321),
            };

            YamlHelper.WriteFile(Path.Combine(configDirectoryPath, "xeus-service.yaml"), xeusServiceConfig);
            YamlHelper.WriteFile(Path.Combine(configDirectoryPath, "deamon.yaml"), deamonConfig);
        }

        [Command("run")]
        public async ValueTask RunAsync([Option('d')] string directory)
        {
            var configDirectoryPath = Path.Combine(directory, ".config");
            if (!Directory.Exists(configDirectoryPath)) Directory.CreateDirectory(configDirectoryPath);

            var xeusServiceConfigPath = Path.Combine(configDirectoryPath, "xeus-service.yaml");
            var deamonConfigPath = Path.Combine(configDirectoryPath, "deamon.yaml");

            var xeusServiceConfig = YamlHelper.ReadFile<XeusServiceConfig>(xeusServiceConfigPath);
            var deamonConfig = YamlHelper.ReadFile<DeamonConfig>(deamonConfigPath);

            var options = new XeusServiceOptions
            {
                Socks5ProxyClientFactory = Socks5ProxyClient.Factory,
                HttpProxyClientFactory = HttpProxyClient.Factory,
                UpnpClientFactory = UpnpClient.Factory,
                TcpConnectorFactory = TcpConnector.Factory,
                NodeFinderFactory = NodeFinder.Factory,
                ContentExchangerFactory = ContentExchanger.Factory,
                DeclaredMessageExchangerFactory = DeclaredMessageExchanger.Factory,
                BytesPool = BytesPool.Shared,
                Config = xeusServiceConfig,
            };

            var service = await XeusService.CreateAsync(options);

            var listenAddress = (OmniAddress?)deamonConfig?.ListenAddress;
            if (listenAddress is null || !listenAddress.TryParseTcpEndpoint(out var ipAddress, out var port))
            {
                _logger.Error($"(ListenAddress) load is failed. \"{deamonConfigPath}\"");
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
                                MaxSendByteCount = 1024 * 64,
                                MaxReceiveByteCount = 1024 * 64,
                                BytesPool = BytesPool.Shared,
                            };
                            var connection = new BaseConnection(cap, dispatcher, connectionOption);

                            var receiver = new XeusServiceReceiver(service, connection, BytesPool.Shared);
                            receiverTasks.Add(receiver.EventLoop().ContinueWith(async (_) => await receiver.DisposeAsync()));
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
