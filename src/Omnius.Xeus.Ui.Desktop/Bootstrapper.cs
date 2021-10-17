using System;
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
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private AppConfig? _config;
        private AppSettings? _appSettings;
        private UiState? _uiState;
        private BytesPool? _bytesPool;
        private XeusServiceManager? _xeusServiceManager;
        private Dashboard? _dashboard;
        private FileDownloader? _fileDownloader;
        private FileUploader? _fileUploader;

        public static Bootstrapper Instance { get; } = new Bootstrapper();

        private Task<ServiceProvider?>? _buildTask;
        private CancellationTokenSource _cancellationTokenSource = new();

        public Bootstrapper()
        {
        }

        public void Build(string configPath, string storageDirectoryPath)
        {
            _buildTask = this.BuildAsync(configPath, storageDirectoryPath);
        }

        private async Task<ServiceProvider?> BuildAsync(string configPath, string storageDirectoryPath, CancellationToken cancellationToken = default)
        {
            try
            {
                _config = await LoadConfigAsync(configPath, cancellationToken);
                _appSettings = await LoadAppSettingsAsync(storageDirectoryPath, cancellationToken);
                _uiState = await LoadUiStateAsync(storageDirectoryPath, cancellationToken);

                _bytesPool = BytesPool.Shared;

                _xeusServiceManager = new XeusServiceManager();
                await _xeusServiceManager.ConnectAsync(OmniAddress.Parse(_config.DaemonAddress), _bytesPool, cancellationToken);

                var xeusService = _xeusServiceManager.GetService();

                _dashboard = await Dashboard.CreateAsync(xeusService, _bytesPool, cancellationToken);

                var fileUploaderOptions = new FileUploaderOptions(Path.Combine(storageDirectoryPath, "file_uploader"));
                _fileUploader = await FileUploader.CreateAsync(xeusService, LiteDatabaseBytesStorage.Factory, _bytesPool, fileUploaderOptions, cancellationToken);

                var fileDownloaderOptions = new FileDownloaderOptions(Path.Combine(storageDirectoryPath, "file_downloader"));
                _fileDownloader = await FileDownloader.CreateAsync(xeusService, LiteDatabaseBytesStorage.Factory, _bytesPool, fileDownloaderOptions, cancellationToken);

                var serviceCollection = new ServiceCollection();

                serviceCollection.AddSingleton(_config);
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

                return serviceCollection.BuildServiceProvider();
            }
            catch (OperationCanceledException e)
            {
                _logger.Debug(e);

                return null;
            }
            catch (Exception e)
            {
                _logger.Error(e);

                throw;
            }
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await _buildTask!;
            _cancellationTokenSource.Dispose();

            if (_xeusServiceManager is not null)
            {
                await _xeusServiceManager.DisposeAsync();
            }
        }

        public async ValueTask<ServiceProvider?> GetServiceProvider()
        {
            return await _buildTask!;
        }

        private static async ValueTask<AppConfig> LoadConfigAsync(string configPath, CancellationToken cancellationToken = default)
        {
            var appConfig = await AppConfig.LoadAsync(configPath);
            if (appConfig is not null)
            {
                return appConfig;
            }

            appConfig = new AppConfig()
            {
                Version = 1,
                DaemonAddress = OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 32321).ToString(),
            };

            await appConfig.SaveAsync(configPath);

            return appConfig;
        }

        private static async ValueTask<AppSettings> LoadAppSettingsAsync(string storageDirectoryPath, CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(storageDirectoryPath, "app_settings.json");
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

        private static async ValueTask<UiState> LoadUiStateAsync(string storageDirectoryPath, CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(storageDirectoryPath, "ui_state.json");
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
