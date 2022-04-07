using Microsoft.Extensions.DependencyInjection;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Axis.Ui.Desktop.Views.Dialogs;
using Omnius.Axis.Ui.Desktop.Views.Main;
using Omnius.Axis.Ui.Desktop.Views.Settings;
using Omnius.Core;
using Omnius.Core.Avalonia;

namespace Omnius.Axis.Ui.Desktop.Internal;

public partial class Bootstrapper : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private AxisEnvironment? _axisEnvironment;
    private ServiceProvider? _serviceProvider;

    public static Bootstrapper Instance { get; } = new Bootstrapper();

    private const string UI_STATUS_FILE_NAME = "ui_status.json";

    private Bootstrapper()
    {
    }

    public async ValueTask BuildAsync(AxisEnvironment axisEnvironment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(axisEnvironment);

        _axisEnvironment = axisEnvironment;

        try
        {
            var bytesPool = BytesPool.Shared;

            var uiState = await UiStatus.LoadAsync(Path.Combine(_axisEnvironment.DatabaseDirectoryPath, UI_STATUS_FILE_NAME));

            var intaractorProvider = new IntaractorProvider(_axisEnvironment.DatabaseDirectoryPath, _axisEnvironment.ListenAddress, bytesPool);

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(_axisEnvironment);
            serviceCollection.AddSingleton<IBytesPool>(bytesPool);
            serviceCollection.AddSingleton(uiState);
            serviceCollection.AddSingleton<IIntaractorProvider>(intaractorProvider);

            serviceCollection.AddSingleton<IApplicationDispatcher, ApplicationDispatcher>();
            serviceCollection.AddSingleton<IMainWindowProvider, MainWindowProvider>();
            serviceCollection.AddSingleton<IClipboardService, ClipboardService>();
            serviceCollection.AddSingleton<IDialogService, DialogService>();
            serviceCollection.AddSingleton<INodesFetcher, NodesFetcher>();

            serviceCollection.AddSingleton<MainWindowViewModel>();
            serviceCollection.AddSingleton<SettingsWindowViewModel>();
            serviceCollection.AddSingleton<MultiLineTextInputWindowViewModel>();
            serviceCollection.AddSingleton<StatusControlViewModel>();
            serviceCollection.AddSingleton<PeersControlViewModel>();
            serviceCollection.AddSingleton<DownloadControlViewModel>();
            serviceCollection.AddSingleton<UploadControlViewModel>();
            serviceCollection.AddSingleton<SignaturesControlViewModel>();

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e, "Operation Canceled");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");

            throw;
        }
    }

    protected override async ValueTask OnDisposeAsync()
    {
        if (_axisEnvironment is null) return;
        if (_serviceProvider is null) return;

        var uiStatus = _serviceProvider.GetRequiredService<UiStatus>();
        await uiStatus.SaveAsync(Path.Combine(_axisEnvironment.DatabaseDirectoryPath, UI_STATUS_FILE_NAME));

        await _serviceProvider.GetRequiredService<IIntaractorProvider>().DisposeAsync();
    }

    public ServiceProvider GetServiceProvider()
    {
        return _serviceProvider ?? throw new NullReferenceException();
    }
}
