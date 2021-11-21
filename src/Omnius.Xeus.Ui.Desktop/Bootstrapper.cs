using System.Net;
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

namespace Omnius.Xeus.Ui.Desktop;

public partial class Bootstrapper : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private string? _configPath;
    private string? _storageDirectoryPath;

    private AppConfig? _appConfig;
    private AppSettings? _appSettings;
    private UiState? _uiState;
    private BytesPool? _bytesPool;
    private ServiceManager? _serviceManager;
    private Dashboard? _dashboard;
    private FileDownloader? _fileDownloader;
    private FileUploader? _fileUploader;
    private ProfilePublisher? _profilePublisher;
    private ProfileSubscriber? _profileSubscriber;

    private Task<ServiceProvider?>? _buildTask;
    private CancellationTokenSource _cancellationTokenSource = new();

    public static Bootstrapper Instance { get; } = new Bootstrapper();

    private Bootstrapper()
    {
    }

    public void Build(string configPath, string storageDirectoryPath)
    {
        _configPath = configPath;
        _storageDirectoryPath = storageDirectoryPath;

        _buildTask = this.BuildAsync(_cancellationTokenSource.Token);
    }

    private async Task<ServiceProvider?> BuildAsync(CancellationToken cancellationToken = default)
    {
        if (_configPath is null) throw new NullReferenceException(nameof(_configPath));
        if (_storageDirectoryPath is null) throw new NullReferenceException(nameof(_storageDirectoryPath));

        try
        {
            _appConfig = await LoadAppConfigAsync(_configPath, cancellationToken);
            _appSettings = await LoadAppSettingsAsync(_storageDirectoryPath, cancellationToken);
            _uiState = await LoadUiStateAsync(_storageDirectoryPath, cancellationToken);

            _bytesPool = BytesPool.Shared;

            _serviceManager = new ServiceManager();
            await _serviceManager.ConnectAsync(OmniAddress.Parse(_appConfig.DaemonAddress), _bytesPool, cancellationToken);

            var service = _serviceManager.GetService();

            _dashboard = await Dashboard.CreateAsync(service, _bytesPool, cancellationToken);

            var fileUploaderOptions = new FileUploaderOptions(Path.Combine(_storageDirectoryPath, "file_uploader"));
            _fileUploader = await FileUploader.CreateAsync(service, KeyValueLiteDatabaseStorage.Factory, _bytesPool, fileUploaderOptions, cancellationToken);

            var fileDownloaderOptions = new FileDownloaderOptions(Path.Combine(_storageDirectoryPath, "file_downloader"));
            _fileDownloader = await FileDownloader.CreateAsync(service, SingleValueFileStorage.Factory, KeyValueLiteDatabaseStorage.Factory, _bytesPool, fileDownloaderOptions, cancellationToken);

            var profilePublisherOptions = new ProfilePublisherOptions(Path.Combine(_storageDirectoryPath, "profile_publisher"));
            _profilePublisher = await ProfilePublisher.CreateAsync(service, KeyValueLiteDatabaseStorage.Factory, _bytesPool, profilePublisherOptions);

            var profileSubscriberOptions = new ProfileSubscriberOptions(Path.Combine(_storageDirectoryPath, "profile_subscriber"));
            _profileSubscriber = await ProfileSubscriber.CreateAsync(service, SingleValueFileStorage.Factory, KeyValueLiteDatabaseStorage.Factory, _bytesPool, profileSubscriberOptions);

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(_appConfig);
            serviceCollection.AddSingleton(_appSettings);
            serviceCollection.AddSingleton(_uiState);

            serviceCollection.AddSingleton<IBytesPool>(_bytesPool);

            serviceCollection.AddSingleton(service);

            serviceCollection.AddSingleton<IDashboard>(_dashboard);
            serviceCollection.AddSingleton<IFileUploader>(_fileUploader);
            serviceCollection.AddSingleton<IFileDownloader>(_fileDownloader);
            serviceCollection.AddSingleton<IProfilePublisher>(_profilePublisher);
            serviceCollection.AddSingleton<IProfileSubscriber>(_profileSubscriber);

            serviceCollection.AddSingleton<IApplicationDispatcher, ApplicationDispatcher>();
            serviceCollection.AddSingleton<IMainWindowProvider, MainWindowProvider>();
            serviceCollection.AddSingleton<IClipboardService, ClipboardService>();
            serviceCollection.AddSingleton<IDialogService, DialogService>();

            serviceCollection.AddTransient<MainWindowViewModel>();
            serviceCollection.AddTransient<SettingsWindowViewModel>();
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

        if (_serviceManager is not null)
        {
            await _serviceManager.DisposeAsync();
        }

        await this.SaveAsync();
    }

    public async ValueTask<ServiceProvider?> GetServiceProvider()
    {
        return await _buildTask!;
    }

    public async ValueTask SaveAsync(CancellationToken cancellationToken = default)
    {
        if (_storageDirectoryPath is null) throw new NullReferenceException(nameof(_storageDirectoryPath));
        if (_appSettings is null) throw new NullReferenceException(nameof(_appSettings));
        if (_uiState is null) throw new NullReferenceException(nameof(_uiState));

        await SaveAppSettingsAsync(_storageDirectoryPath, _appSettings, cancellationToken);
        await SaveUiStateAsync(_storageDirectoryPath, _uiState, cancellationToken);
    }

    private static async ValueTask<AppConfig> LoadAppConfigAsync(string configPath, CancellationToken cancellationToken = default)
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

    private static async ValueTask SaveAppSettingsAsync(string storageDirectoryPath, AppSettings appSettings, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(storageDirectoryPath, "app_settings.json");
        await appSettings.SaveAsync(filePath);
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

    private static async ValueTask SaveUiStateAsync(string storageDirectoryPath, UiState uiState, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(storageDirectoryPath, "ui_state.json");
        await uiState.SaveAsync(filePath);
    }
}
