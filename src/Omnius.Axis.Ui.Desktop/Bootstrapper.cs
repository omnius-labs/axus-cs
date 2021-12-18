using Microsoft.Extensions.DependencyInjection;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Axis.Ui.Desktop.Controls;
using Omnius.Axis.Ui.Desktop.Windows;
using Omnius.Core;
using Omnius.Core.Avalonia;

namespace Omnius.Axis.Ui.Desktop;

public partial class Bootstrapper : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private AppConfig? _appConfig;

    private UiStatus? _uiState;

    private Task<ServiceProvider?>? _buildTask;
    private CancellationTokenSource _cancellationTokenSource = new();

    public static Bootstrapper Instance { get; } = new Bootstrapper();

    private Bootstrapper()
    {
    }

    public void Build(AppConfig appConfig)
    {
        _appConfig = appConfig;
        _buildTask = this.BuildAsync(_cancellationTokenSource.Token);
    }

    private async Task<ServiceProvider?> BuildAsync(CancellationToken cancellationToken = default)
    {
        if (_appConfig is null) throw new NullReferenceException(nameof(_appConfig));

        try
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(_appConfig);

            _uiState = await UiStatus.LoadAsync(Path.Combine(_appConfig.DatabaseDirectoryPath!, "ui_state.json"));
            serviceCollection.AddSingleton(_uiState);

            serviceCollection.AddSingleton<IBytesPool>(BytesPool.Shared);

            serviceCollection.AddSingleton<IIntaractorProvider, IntaractorProvider>();
            serviceCollection.AddSingleton<IDialogService, DialogService>();

            serviceCollection.AddSingleton<IApplicationDispatcher, ApplicationDispatcher>();
            serviceCollection.AddSingleton<IMainWindowProvider, MainWindowProvider>();
            serviceCollection.AddSingleton<IClipboardService, ClipboardService>();

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

        await this.SaveAsync();
    }

    public async ValueTask SaveAsync(CancellationToken cancellationToken = default)
    {
        if (_uiState is null) throw new NullReferenceException(nameof(_uiState));
        await _uiState.SaveAsync(Path.Combine(_appConfig!.DatabaseDirectoryPath!, "ui_state.json"));
    }

    public async ValueTask<ServiceProvider?> GetServiceProvider()
    {
        return await _buildTask!;
    }
}
