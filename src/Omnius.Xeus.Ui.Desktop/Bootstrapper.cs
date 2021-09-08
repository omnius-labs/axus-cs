using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Omnius.Core.Net;
using Omnius.Xeus.Intaractors;
using Omnius.Xeus.Ui.Desktop.Configuration;
using Omnius.Xeus.Ui.Desktop.Internal;
using Omnius.Xeus.Ui.Desktop.Windows;
using Omnius.Xeus.Ui.Desktop.Windows.Controls;

namespace Omnius.Xeus.Ui.Desktop
{
    public partial class Bootstrapper : AsyncDisposableBase
    {
        private BytesPool? _bytesPool;
        private AppConfig? _appConfig;
        private AppSettings? _appSettings;
        private UiState? _uiState;
        private XeusServiceManager? _xeusServiceManager;

        public static Bootstrapper Instance { get; } = new Bootstrapper();

        public Bootstrapper()
        {
        }

        public async ValueTask BuildAsync(string stateDirectoryPath, CancellationToken cancellationToken = default)
        {
            _bytesPool = BytesPool.Shared;

            _appConfig = await LoadAppConfigAsync(stateDirectoryPath, cancellationToken);
            _appSettings = await LoadAppSettingsAsync(stateDirectoryPath, cancellationToken);
            _uiState = await LoadUiStateAsync(stateDirectoryPath, cancellationToken);

            _xeusServiceManager = new XeusServiceManager();
            await _xeusServiceManager.BuildAsync(OmniAddress.Parse(_appConfig.DaemonAddress), _bytesPool, cancellationToken);

            var serviceCollection = new ServiceCollection();

            _ = serviceCollection.AddSingleton<IBytesPool>(_bytesPool);

            _ = serviceCollection.AddSingleton(_appConfig);
            _ = serviceCollection.AddSingleton(_appSettings);
            _ = serviceCollection.AddSingleton(_uiState);

            _ = serviceCollection.AddSingleton(_xeusServiceManager.XeusService);

            _ = serviceCollection.AddSingleton<IDashboard, Dashboard>();

            _ = serviceCollection.AddSingleton<IApplicationDispatcher, ApplicationDispatcher>();
            _ = serviceCollection.AddSingleton<IMainWindowProvider, MainWindowProvider>();
            _ = serviceCollection.AddSingleton<IClipboardService, ClipboardService>();

            _ = serviceCollection.AddSingleton<MainWindowViewModel>();
            _ = serviceCollection.AddSingleton<StatusControlViewModel>();
            _ = serviceCollection.AddSingleton<PeersControlViewModel>();

            this.ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        protected override async ValueTask OnDisposeAsync()
        {
            if (_xeusServiceManager is not null)
            {
                await _xeusServiceManager.DisposeAsync();
            }
        }

        public ServiceProvider? ServiceProvider { get; private set; }

        private static async ValueTask<AppConfig> LoadAppConfigAsync(string stateDirectoryPath, CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(stateDirectoryPath, "config.yml");
            var appConfig = await AppConfig.LoadAsync(filePath);
            if (appConfig is not null)
            {
                return appConfig;
            }

            appConfig = new AppConfig()
            {
                DaemonAddress = OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 32321).ToString(),
            };

            await appConfig.SaveAsync(filePath);

            return appConfig;
        }

        private static async ValueTask<AppSettings> LoadAppSettingsAsync(string stateDirectoryPath, CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(stateDirectoryPath, "app_settings.json");
            var appSettings = await AppSettings.LoadAsync(filePath);
            if (appSettings is not null)
            {
                return appSettings;
            }

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
            if (uiState is not null)
            {
                return uiState;
            }

            uiState = new UiState
            {
            };

            await uiState.SaveAsync(filePath);

            return uiState;
        }
    }
}
