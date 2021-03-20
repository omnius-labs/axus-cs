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
        private UiSettings _uiSettings = null!;
        private Options _options = null!;
        private IDashboard _dashboard = null!;

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

        private string GetConfigFilePath() => Path.Combine(_stateDirectoryPath, "config.yml");

        private string GetUiSettingsFilePath() => Path.Combine(_stateDirectoryPath, "ui_settings.json");

        private string GetOptionsFilePath() => Path.Combine(_stateDirectoryPath, "options.json");

        private async ValueTask InitAsync(CancellationToken cancellationToken = default)
        {
            var config = await this.LoadConfigAsync(cancellationToken);
            _xeusService = await this.CreateXeusServiceAsync(config, cancellationToken);

            _uiSettings = await this.LoadUiSettingsAsync(cancellationToken);

            var option = await this.LoadOptionsAsync(cancellationToken);
            await this.UpdateOptionsAsync(option, cancellationToken);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            await this.SaveAsync();
        }

        private async ValueTask SaveAsync()
        {
            await _uiSettings.SaveAsync(this.GetUiSettingsFilePath());
        }

        public IBytesPool GetBytesPool() => _bytesPool;

        public IXeusService GetXeusService() => _xeusService;

        public UiSettings GetUiSettings() => _uiSettings;

        public Options GetOptions()
        {
            using (_asyncLock.ReaderLock())
            {
                return _options;
            }
        }

        public IDashboard GetDashboard()
        {
            using (_asyncLock.ReaderLock())
            {
                return _dashboard;
            }
        }

        private async ValueTask<Config> LoadConfigAsync(CancellationToken cancellationToken = default)
        {
            var config = await Config.LoadAsync(this.GetConfigFilePath());
            if (config is not null) return config;

            config = new Config()
            {
                DaemonAddress = OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 40001).ToString(),
            };

            await config.SaveAsync(this.GetConfigFilePath());

            return config;
        }

        private async ValueTask<UiSettings> LoadUiSettingsAsync(CancellationToken cancellationToken = default)
        {
            var uiSettings = await UiSettings.LoadAsync(this.GetUiSettingsFilePath());

            if (uiSettings is null)
            {
                uiSettings = new UiSettings
                {
                };
            }

            return uiSettings;
        }

        private async ValueTask<IXeusService> CreateXeusServiceAsync(Config config, CancellationToken cancellationToken = default)
        {
            if (config.DaemonAddress is null) throw new Exception("DaemonAddress is not found.");

            var daemonAddress = new OmniAddress(config.DaemonAddress);
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
            for (; ; )
            {
                cancellationToken.ThrowIfCancellationRequested();

                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(new IPEndPoint(ipAddress, port), TimeSpan.FromSeconds(3), cancellationToken);
                return socket;
            }
        }

        private async ValueTask<Options> LoadOptionsAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.ReaderLockAsync(cancellationToken))
            {
                var options = await Options.LoadAsync(this.GetOptionsFilePath());

                if (options is null)
                {
                    options = new Options();
                }

                return options;
            }
        }

        public async ValueTask UpdateOptionsAsync(Options options, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WriterLockAsync(cancellationToken))
            {
                if (_dashboard is not null)
                {
                    await _dashboard.DisposeAsync();
                }

                _dashboard = await Dashboard.Factory.CreateAsync(_xeusService, _bytesPool, cancellationToken);

                await options.SaveAsync(this.GetOptionsFilePath());

                _options = options;
            }
        }
    }
}
