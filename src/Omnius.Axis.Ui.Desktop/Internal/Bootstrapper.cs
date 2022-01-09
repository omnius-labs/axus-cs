using Microsoft.Extensions.DependencyInjection;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Axis.Ui.Desktop.Views.Dialogs;
using Omnius.Axis.Ui.Desktop.Views.Main;
using Omnius.Axis.Ui.Desktop.Views.Settings;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Omnius.Core.Net;

namespace Omnius.Axis.Ui.Desktop.Internal;

public partial class Bootstrapper : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private string? _databaseDirectoryPath;
    private OmniAddress? _listenAddress;

    private UiStatus? _uiState;
    private IntaractorProvider? _intaractorProvider;

    private Task<ServiceProvider?>? _buildTask;
    private CancellationTokenSource _cancellationTokenSource = new();

    public static Bootstrapper Instance { get; } = new Bootstrapper();

    private const string UI_STATE_FILE_NAME = "ui_state.json";

    private Bootstrapper()
    {
    }

    public void Build(string databaseDirectoryPath, OmniAddress listenAddress)
    {
        _databaseDirectoryPath = databaseDirectoryPath;
        _listenAddress = listenAddress;

        _buildTask = this.BuildAsync(_cancellationTokenSource.Token);
    }

    private async Task<ServiceProvider?> BuildAsync(CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(_databaseDirectoryPath);
        ArgumentNullException.ThrowIfNull(_listenAddress);

        try
        {
            _uiState = await UiStatus.LoadAsync(Path.Combine(_databaseDirectoryPath, UI_STATE_FILE_NAME));
            _intaractorProvider = new IntaractorProvider(_databaseDirectoryPath, _listenAddress, BytesPool.Shared);

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(_uiState);
            serviceCollection.AddSingleton<IIntaractorProvider>(_intaractorProvider);
            serviceCollection.AddSingleton<IBytesPool>(BytesPool.Shared);

            serviceCollection.AddSingleton<IApplicationDispatcher, ApplicationDispatcher>();
            serviceCollection.AddSingleton<IMainWindowProvider, MainWindowProvider>();
            serviceCollection.AddSingleton<IClipboardService, ClipboardService>();
            serviceCollection.AddSingleton<IDialogService, DialogService>();

            serviceCollection.AddTransient<MainWindowViewModel>();
            serviceCollection.AddTransient<SettingsWindowViewModel>();
            serviceCollection.AddTransient<MultiLineTextBoxWindowViewModel>();
            serviceCollection.AddTransient<StatusControlViewModel>();
            serviceCollection.AddTransient<PeersControlViewModel>();
            serviceCollection.AddTransient<DownloadControlViewModel>();
            serviceCollection.AddTransient<UploadControlViewModel>();
            serviceCollection.AddTransient<SignaturesControlViewModel>();

            return serviceCollection.BuildServiceProvider();
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);

            throw;
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
        if (_buildTask is not null) await _buildTask;
        _cancellationTokenSource.Dispose();

        if (_uiState is not null) await _uiState.SaveAsync(Path.Combine(_databaseDirectoryPath!, UI_STATE_FILE_NAME));
    }

    public ServiceProvider GetServiceProvider()
    {
        return _buildTask?.Result ?? throw new NullReferenceException();
    }
}
