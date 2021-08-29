using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Omnius.Core.Extensions;
using Omnius.Core.Net;
using Omnius.Core.Net.Caps;
using Omnius.Core.Net.Connections;
using Omnius.Xeus.Service.Daemon;
using Omnius.Xeus.Intaractors;
using Omnius.Xeus.Ui.Desktop.Configuration;
using Omnius.Xeus.Ui.Desktop.Windows;
using Omnius.Xeus.Ui.Desktop.Windows.AddNodes;
using Omnius.Xeus.Ui.Desktop.Windows.Main;
using Omnius.Xeus.Ui.Desktop.Windows.Main.Peers;
using Omnius.Xeus.Ui.Desktop.Windows.Main.Status;

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

            var config = await LoadConfigAsync(stateDirectoryPath, cancellationToken);
            serviceCollection.AddSingleton(config);

            var appSettings = await LoadAppSettingsAsync(stateDirectoryPath, cancellationToken);
            serviceCollection.AddSingleton(appSettings);

            var uiState = await LoadUiStateAsync(stateDirectoryPath, cancellationToken);
            serviceCollection.AddSingleton(uiState);

            var xeusService = await CreateXeusServiceAsync(config, bytesPool, cancellationToken);
            serviceCollection.AddSingleton(xeusService);

            var dashboard = await Dashboard.Factory.CreateAsync(xeusService, bytesPool, cancellationToken);
            serviceCollection.AddSingleton(dashboard);

            serviceCollection.AddSingleton<IApplicationDispatcher, ApplicationDispatcher>();
            serviceCollection.AddSingleton<IMainWindowProvider, MainWindowProvider>();
            serviceCollection.AddSingleton<IDialogService, DialogService>();
            serviceCollection.AddSingleton<IClipboardService, ClipboardService>();

            serviceCollection.AddTransient<MainWindowViewModel>();
            serviceCollection.AddTransient<StatusControlViewModel>();
            serviceCollection.AddTransient<PeersControlViewModel>();
            serviceCollection.AddTransient<AddNodesWindowViewModel>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private static async ValueTask<AppConfig> LoadConfigAsync(string stateDirectoryPath, CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(stateDirectoryPath, "config.yml");
            var config = await AppConfig.LoadAsync(filePath);
            if (config is not null) return config;

            config = new AppConfig()
            {
                DaemonAddress = OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 32321).ToString(),
            };

            await config.SaveAsync(filePath);

            return config;
        }

        private static async ValueTask<AppSettings> LoadAppSettingsAsync(string stateDirectoryPath, CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(stateDirectoryPath, "app_settings.json");
            var appSettings = await AppSettings.LoadAsync(filePath);
            if (appSettings is not null) return appSettings;

            appSettings = new AppSettings()
            {
            };

            await appSettings.SaveAsync(filePath);

            return appSettings;
        }

        private static async ValueTask<UiState> LoadUiStateAsync(string stateDirectoryPath, CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(stateDirectoryPath, "ui_state.json");
            var uiState = await UiState.LoadAsync(filePath);
            if (uiState is not null) return uiState;

            uiState = new UiState
            {
            };

            await uiState.SaveAsync(filePath);

            return uiState;
        }

        private static async ValueTask<IXeusService> CreateXeusServiceAsync(AppConfig config, IBytesPool bytesPool, CancellationToken cancellationToken = default)
        {
            if (config.DaemonAddress is null) throw new Exception("DaemonAddress is not found.");

            var daemonAddress = new OmniAddress(config.DaemonAddress);
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

            var service = new XeusServiceRemoting.Client(baseConnection, bytesPool);
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
