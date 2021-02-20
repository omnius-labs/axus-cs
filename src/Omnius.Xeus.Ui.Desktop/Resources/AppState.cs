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
using Omnius.Xeus.Service.Interactors;
using Omnius.Xeus.Service.Presenters;
using Omnius.Xeus.Ui.Desktop.Resources.Models;

namespace Omnius.Xeus.Ui.Desktop.Resources
{
    public class AppState : AsyncDisposableBase
    {
        private readonly string _stateDirectoryPath;
        private readonly IBytesPool _bytesPool;

        private Options _options = null!;
        private UiSettings _uiSettings = null!;
        private IXeusService _xeusService = null!;
        private IUserProfileSubscriber? _userProfileSubscriber;
        private IUserProfileFinder? _userProfileFinder;

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

        private string GetOptionsFilePath() => Path.Combine(_stateDirectoryPath, "options.json");

        private string GetUiSettingsFilePath() => Path.Combine(_stateDirectoryPath, "ui_settings.json");

        private async ValueTask InitAsync(CancellationToken cancellationToken = default)
        {
            var config = await this.CreateConfigAsync(cancellationToken);
            _xeusService = await this.CreateXeusServiceAsync(config, cancellationToken);

            var option = await this.CreateOptionsAsync(cancellationToken);
            await this.UpdateOptionsAsync(option, cancellationToken);

            _uiSettings = await this.CreateUiSettingsAsync(cancellationToken);
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

        public Options GetOptions() => _options;

        public UiSettings GetUiSettings() => _uiSettings;

        public IXeusService GetXeusService() => _xeusService;

        private IUserProfileSubscriber? GetUserProfileSubscriber() => _userProfileSubscriber;

        private IUserProfileFinder? GetUserProfileFinder() => _userProfileFinder;

        private async ValueTask<Config> CreateConfigAsync(CancellationToken cancellationToken = default)
        {
            var config = await Config.LoadAsync(this.GetConfigFilePath());
            if (config is not null) return config;

            config = new Config()
            {
                DaemonAddress = (string?)OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 32321),
            };

            return config;
        }

        private async ValueTask<UiSettings> CreateUiSettingsAsync(CancellationToken cancellationToken = default)
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
            if (!daemonAddress.TryParseTcpEndpoint(out var ipAddress, out var port)) throw new Exception("DaemonAddress is invalid format.");

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(new IPEndPoint(ipAddress, port), TimeSpan.FromSeconds(3), cancellationToken);

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

        private async ValueTask<Options> CreateOptionsAsync(CancellationToken cancellationToken = default)
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
                if (_userProfileFinder is not null)
                {
                    await _userProfileFinder.DisposeAsync();
                }

                if (_userProfileSubscriber is not null)
                {
                    await _userProfileSubscriber.DisposeAsync();
                }

                var userProfileSubscriberOptions = new UserProfileSubscriberOptions(
                    Path.Combine(_stateDirectoryPath, "omnius.xeus.service.interactors", "user_profile_subscriber"),
                    _xeusService,
                    _bytesPool);
                _userProfileSubscriber = await UserProfileSubscriber.Factory.CreateAsync(userProfileSubscriberOptions);

                var userProfileFinderOptions = new UserProfileFinderOptions(
                    options.TrustedSignatures,
                    options.BlockedSignatures,
                    options.SearchProfileDepth,
                    Path.Combine(_stateDirectoryPath, "omnius.xeus.service.interactors", "user_profile_finder"),
                    _userProfileSubscriber,
                    _bytesPool);
                _userProfileFinder = await UserProfileFinder.Factory.CreateAsync(userProfileFinderOptions);

                await options.SaveAsync(this.GetOptionsFilePath());

                _options = options;
            }
        }
    }
}
