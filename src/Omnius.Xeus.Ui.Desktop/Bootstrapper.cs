using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Core;
using Omnius.Core.Extensions;
using Omnius.Core.Net;
using Omnius.Core.Net.Caps;
using Omnius.Core.Net.Connections;
using Omnius.Xeus.Api;
using Omnius.Xeus.Interactors;
using Omnius.Xeus.Ui.Desktop.Configuration;
using Omnius.Xeus.Ui.Desktop.Windows.Main;
using Omnius.Xeus.Ui.Desktop.Windows.Main.Peers;

namespace Omnius.Xeus.Ui.Desktop
{
    public static class Bootstrapper
    {
        public static ServiceProvider? ServiceProvider { get; private set; }

        public static async ValueTask RegisterAsync(string stateDirectoryPath, CancellationToken cancellationToken = default)
        {
            var serviceCollection = new ServiceCollection();

            var bytesPool = BytesPool.Shared;
            serviceCollection.AddSingleton<IBytesPool>(bytesPool);

            var appSettings = await LoadAppSettingsAsync(stateDirectoryPath, cancellationToken);
            serviceCollection.AddSingleton(appSettings);

            var uiState = await LoadUiStateAsync(stateDirectoryPath, cancellationToken);
            serviceCollection.AddSingleton(uiState);

            var xeusService = await CreateXeusServiceAsync(appSettings, bytesPool, cancellationToken);
            serviceCollection.AddSingleton(xeusService);

            var dashboard = await Dashboard.Factory.CreateAsync(xeusService, bytesPool, cancellationToken);
            serviceCollection.AddSingleton(dashboard);

            serviceCollection.AddSingleton<IMainWindowViewModel, MainWindowViewModel>();
            serviceCollection.AddSingleton<IPeersControlViewModel, PeersControlViewModel>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private static async ValueTask<AppSettings> LoadAppSettingsAsync(string stateDirectoryPath, CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(stateDirectoryPath, "app_settings.json");
            var appSettings = await AppSettings.LoadAsync(filePath);

            if (appSettings is null)
            {
                appSettings = new AppSettings()
                {
                    DaemonAddress = OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 40001).ToString(),
                };

                await appSettings.SaveAsync(filePath);
            }

            return appSettings;
        }

        private static async ValueTask<UiState> LoadUiStateAsync(string stateDirectoryPath, CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(stateDirectoryPath, "ui_state.json");
            var uiState = await UiState.LoadAsync(filePath);

            if (uiState is null)
            {
                uiState = new UiState
                {
                };

                await uiState.SaveAsync(filePath);
            }

            return uiState;
        }

        private static async ValueTask<IXeusService> CreateXeusServiceAsync(AppSettings appSettings, IBytesPool bytesPool, CancellationToken cancellationToken = default)
        {
            if (appSettings.DaemonAddress is null) throw new Exception("DaemonAddress is not found.");

            var daemonAddress = new OmniAddress(appSettings.DaemonAddress);
            if (!daemonAddress.TryGetTcpEndpoint(out var ipAddress, out var port)) throw new Exception("DaemonAddress is invalid format.");

            var socket = await ConnectAsync(ipAddress, port, cancellationToken);

            var cap = new SocketCap(socket);

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

        private static async ValueTask<Socket> ConnectAsync(IPAddress ipAddress, ushort port, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(new IPEndPoint(ipAddress, port), TimeSpan.FromSeconds(3), cancellationToken);
            return socket;
        }
    }
}
