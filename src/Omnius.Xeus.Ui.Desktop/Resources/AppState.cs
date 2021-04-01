using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Omnius.Core;
using Omnius.Core.Extensions;
using Omnius.Core.Network;
using Omnius.Core.Network.Caps;
using Omnius.Core.Network.Connections;
using Omnius.Lxna.Ui.Desktop.Resources.Models;
using Omnius.Xeus.Api;
using Omnius.Xeus.Interactors;
using Omnius.Xeus.Ui.Desktop.Resources.Models;

namespace Omnius.Xeus.Ui.Desktop.Resources
{
    public class AppState : AsyncDisposableBase
    {
        private readonly string _stateDirectoryPath;
        private readonly IBytesPool _bytesPool;

        private IXeusService _xeusService = null!;
        private IDashboard _dashboard = null!;
        private IUserProfileFinder _userProfileFinder = null!;
        private IUserProfileDownloader _userProfileDownloader = null!;
        private IUserProfileUploader _userProfileUploader = null!;
        private AppSettings _appSettings = null!;
        private UiState _uiState = null!;

        private readonly AsyncReaderWriterLock _asyncLock = new();

        public class AppStateFactory
        {
            public async ValueTask<AppState> CreateAsync(string stateDirectoryPath, IBytesPool bytesPool)
            {
                var result = new AppState(stateDirectoryPath, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static AppStateFactory Factory { get; } = new();

        private AppState(string stateDirectoryPath, IBytesPool bytesPool)
        {
            _stateDirectoryPath = stateDirectoryPath;
            _bytesPool = bytesPool;
        }

        private string GetAppConfigFilePath() => Path.Combine(_stateDirectoryPath, "app_config.yml");

        private string GetAppSettingsFilePath() => Path.Combine(_stateDirectoryPath, "app_settings.json");

        private string GetUiStateFilePath() => Path.Combine(_stateDirectoryPath, "ui_state.json");

        private async ValueTask InitAsync(CancellationToken cancellationToken = default)
        {
            var appConfig = await this.LoadAppConfigAsync(cancellationToken);
            _xeusService = await this.CreateXeusServiceAsync(appConfig, cancellationToken);
            _dashboard = await Dashboard.Factory.CreateAsync(_xeusService, _bytesPool, cancellationToken);

            _uiState = await this.LoadUiStateAsync(cancellationToken);

            var appSettings = await this.LoadAppSettingsAsync(cancellationToken);
            await this.UpdateAppSettingsAsync(appSettings, cancellationToken);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            await this.SaveAsync();
        }

        private async ValueTask SaveAsync()
        {
            await _uiState.SaveAsync(this.GetUiStateFilePath());
        }

        public IBytesPool GetBytesPool() => _bytesPool;

        public IXeusService GetXeusService() => _xeusService;

        public IDashboard GetDashboard() => _dashboard;

        public AppSettings GetAppSettings()
        {
            using (_asyncLock.ReaderLock())
            {
                return _appSettings;
            }
        }

        public UiState GetUiState() => _uiState;

        private async ValueTask<AppConfig> LoadAppConfigAsync(CancellationToken cancellationToken = default)
        {
            var appConfig = await AppConfig.LoadAsync(this.GetAppConfigFilePath());
            if (appConfig is not null) return appConfig;

            appConfig = new AppConfig()
            {
                DaemonAddress = OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 40001).ToString(),
            };

            await appConfig.SaveAsync(this.GetAppConfigFilePath());

            return appConfig;
        }

        private async ValueTask<UiState> LoadUiStateAsync(CancellationToken cancellationToken = default)
        {
            var uiState = await UiState.LoadAsync(this.GetUiStateFilePath());

            if (uiState is null)
            {
                uiState = new UiState
                {
                };
            }

            return uiState;
        }

        private async ValueTask<IXeusService> CreateXeusServiceAsync(AppConfig appConfig, CancellationToken cancellationToken = default)
        {
            if (appConfig.DaemonAddress is null) throw new Exception("DaemonAddress is not found.");

            var daemonAddress = new OmniAddress(appConfig.DaemonAddress);
            if (!daemonAddress.TryGetTcpEndpoint(out var ipAddress, out var port)) throw new Exception("DaemonAddress is invalid format.");

            var socket = await this.ConnectAsync(ipAddress, port, cancellationToken);

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
                BytesPool = _bytesPool,
            };
            var baseConnection = new BaseConnection(cap, baseConnectionDispatcher, baseConnectionOptions);

            var service = new XeusService.Client(baseConnection, _bytesPool);
            return service;
        }

        private async ValueTask<Socket> ConnectAsync(IPAddress ipAddress, ushort port, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(new IPEndPoint(ipAddress, port), TimeSpan.FromSeconds(3), cancellationToken);
            return socket;
        }

        private async ValueTask<AppSettings> LoadAppSettingsAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var appSettings = await AppSettings.LoadAsync(this.GetAppSettingsFilePath());

                if (appSettings is null)
                {
                    appSettings = new AppSettings();
                }

                return appSettings;
            }
        }

        public async ValueTask UpdateAppSettingsAsync(AppSettings appSettings, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                await appSettings.SaveAsync(this.GetAppSettingsFilePath());
                _appSettings = appSettings;
            }
        }
    }
}
