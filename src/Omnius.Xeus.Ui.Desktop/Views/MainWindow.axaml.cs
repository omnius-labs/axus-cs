using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Omnius.Core;
using Omnius.Core.Extensions;
using Omnius.Core.Network;
using Omnius.Core.Network.Caps;
using Omnius.Core.Network.Connections;
using Omnius.Xeus.Api;
using Omnius.Xeus.Ui.Desktop.Configs;
using Omnius.Xeus.Ui.Desktop.ViewModels;

namespace Omnius.Xeus.Ui.Desktop.Views
{
    public class MainWindow : Window
    {
        private readonly Task _initTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public MainWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            _initTask = this.InitAsync();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async Task InitAsync()
        {
            var xeusService = await this.ConnectAsync();

            this.ViewModel = new MainWindowViewModel();
            this.FileSearchControl.ViewModel = new FileSearchControlViewModel(xeusService);
        }

        private async ValueTask<IXeusService> ConnectAsync()
        {
            var config = UiConfig.LoadConfig("../config");
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
            await socket.ConnectAsync(new IPEndPoint(ipAddress, port), TimeSpan.FromSeconds(3), _cancellationTokenSource.Token);

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

        public MainWindowViewModel? ViewModel
        {
            get => this.DataContext as MainWindowViewModel;
            set => this.DataContext = value;
        }

        private FileSearchControl FileSearchControl => this.FindControl<FileSearchControl>("FileSearchControl");

        protected override async void OnClosed(EventArgs e)
        {
            await _initTask;
            await this.FileSearchControl.ViewModel!.DisposeAsync();
            await this.ViewModel!.DisposeAsync();
        }
    }
}
