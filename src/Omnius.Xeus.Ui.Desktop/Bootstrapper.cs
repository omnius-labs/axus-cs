using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Omnius.Core.Net;
using Omnius.Core.Storages;
using Omnius.Xeus.Intaractors;
using Omnius.Xeus.Ui.Desktop.Configuration;
using Omnius.Xeus.Ui.Desktop.Controls;
using Omnius.Xeus.Ui.Desktop.Internal;
using Omnius.Xeus.Ui.Desktop.Windows;

namespace Omnius.Xeus.Ui.Desktop
{
    public partial class Bootstrapper : AsyncDisposableBase
    {
        private AppConfig? _appConfig;
        private AppSettings? _appSettings;
        private UiState? _uiState;
        private BytesPool? _bytesPool;
        private XeusServiceManager? _xeusServiceManager;
        private Dashboard? _dashboard;
        private FileDownloader _fileDownloader;
        private FileUploader? _fileUploader;

        public static Bootstrapper Instance { get; } = new Bootstrapper();

        public Bootstrapper()
        {
        }

        public async ValueTask BuildAsync(string stateDirectoryPath, CancellationToken cancellationToken = default)
        {
            _appConfig = await LoadAppConfigAsync(stateDirectoryPath, cancellationToken);
            _appSettings = await LoadAppSettingsAsync(stateDirectoryPath, cancellationToken);
            _uiState = await LoadUiStateAsync(stateDirectoryPath, cancellationToken);

            _bytesPool = BytesPool.Shared;

            _xeusServiceManager = new XeusServiceManager();
            await _xeusServiceManager.ConnectAsync(OmniAddress.Parse(_appConfig.DaemonAddress), _bytesPool, cancellationToken);

            var xeusService = _xeusServiceManager.GetService();

            _dashboard = await Dashboard.CreateAsync(xeusService, _bytesPool, cancellationToken);

            var fileUploaderOptions = new FileUploaderOptions(Path.Combine(stateDirectoryPath, "file_uploader"));
            _fileUploader = await FileUploader.CreateAsync(xeusService, LiteDatabaseBytesStorage.Factory, _bytesPool, fileUploaderOptions, cancellationToken);

            var fileDownloaderOptions = new FileDownloaderOptions(Path.Combine(stateDirectoryPath, "file_downloader"));
            _fileDownloader = await FileDownloader.CreateAsync(xeusService, LiteDatabaseBytesStorage.Factory, _bytesPool, fileDownloaderOptions, cancellationToken);

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(_appConfig);
            serviceCollection.AddSingleton(_appSettings);
            serviceCollection.AddSingleton(_uiState);

            serviceCollection.AddSingleton<IBytesPool>(_bytesPool);

            serviceCollection.AddSingleton(xeusService);

            serviceCollection.AddSingleton<IDashboard>(_dashboard);
            serviceCollection.AddSingleton<IFileUploader>(_fileUploader);
            serviceCollection.AddSingleton<IFileDownloader>(_fileDownloader);

            serviceCollection.AddSingleton<IApplicationDispatcher, ApplicationDispatcher>();
            serviceCollection.AddSingleton<IMainWindowProvider, MainWindowProvider>();
            serviceCollection.AddSingleton<IClipboardService, ClipboardService>();
            serviceCollection.AddSingleton<IDialogService, DialogService>();

            serviceCollection.AddTransient<MainWindowViewModel>();
            serviceCollection.AddTransient<TextWindowViewModel>();
            serviceCollection.AddTransient<StatusControlViewModel>();
            serviceCollection.AddTransient<PeersControlViewModel>();
            serviceCollection.AddTransient<DownloadControlViewModel>();
            serviceCollection.AddTransient<UploadControlViewModel>();

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
