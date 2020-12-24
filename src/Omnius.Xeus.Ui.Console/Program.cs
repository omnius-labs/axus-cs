using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Cocona;
using Omnius.Core;
using Omnius.Core.Extensions;
using Omnius.Core.Network;
using Omnius.Core.Network.Caps;
using Omnius.Core.Network.Connections;
using Omnius.Xeus.Api;
using Omnius.Xeus.Ui.Console.Configs;
using Omnius.Xeus.Ui.Console.Internal;

namespace Omnius.Xeus.Ui.Console
{
    public class Program : CoconaLiteConsoleAppBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            CoconaLiteApp.Run<Program>(args);
        }

        [Command("init")]
        public void Init([Option('c')] string configDirectoryPath)
        {
            _logger.Info("init start");

            if (!Directory.Exists(configDirectoryPath))
            {
                Directory.CreateDirectory(configDirectoryPath);
            }

            var consoleConfig = new ConsoleConfig()
            {
                DaemonAddress = (string?)OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 32321),
            };

            YamlHelper.WriteFile(Path.Combine(configDirectoryPath, "console.yaml"), consoleConfig);

            _logger.Info("init end");
        }

        [Command("run")]
        public async ValueTask RunAsync([Option('c')] string configDirectoryPath = "../config")
        {
            _logger.Info("run start");

            var service = await this.ConnectAsync(configDirectoryPath);
            var result = await service.GetMyNodeProfileAsync(this.Context.CancellationToken);
            this.Output(result);

            _logger.Info("run end");
        }

        private async ValueTask<IXeusService> ConnectAsync(string configDirectoryPath)
        {
            var config = this.LoadConfig(configDirectoryPath);
            if (config.DaemonAddress is null)
            {
                throw new Exception("DaemonAddress is not found.");
            }

            var daemonAddress = new OmniAddress(config.DaemonAddress);
            if (!daemonAddress.TryParseTcpEndpoint(out var ipAddress, out var port))
            {
                throw new Exception("DaemonAddress is invalid format.");
            }

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(new IPEndPoint(ipAddress, port), TimeSpan.FromSeconds(3), this.Context.CancellationToken);

            var cap = new SocketCap(socket);

            var bytesPool = BytesPool.Shared;
            var baseConnectionDispatcherOptions = new BaseConnectionDispatcherOptions()
            {
                MaxSendBytesPerSeconds = 32 * 1024 * 1024,
                MaxReceiveBytesPerSeconds = 32 * 1024 * 1024,
            };
            var baseConnectionDispatcher = new BaseConnectionDispatcher(baseConnectionDispatcherOptions);
            var baseConnectionOptions = new BaseConnectionOptions()
            {
                MaxReceiveByteCount = 32 * 1024 * 1024,
                BytesPool = bytesPool,
            };
            var baseConnection = new BaseConnection(cap, baseConnectionDispatcher, baseConnectionOptions);

            var service = new XeusService.Client(baseConnection, bytesPool);
            return service;
        }

        private ConsoleConfig LoadConfig(string configDirectoryPath)
        {
            var configPath = Path.Combine(configDirectoryPath, "console.yaml");
            var config = YamlHelper.ReadFile<ConsoleConfig>(configPath);
            return config;
        }

        private void Output<T>(T value)
        {
            var stream = System.Console.OpenStandardOutput();
            JsonHelper.WriteStream(stream, value);
            using var writer = new StreamWriter(stream, leaveOpen: true);
            writer.WriteLine();
        }
    }
}
