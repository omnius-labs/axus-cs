using Microsoft.Extensions.DependencyInjection;
using Omnius.Axus.Ui.Desktop.Models;
using Omnius.Axus.Ui.Desktop.Windows.Main;
using Omnius.Axus.Ui.Desktop.Windows.Settings;
using Omnius.Axus.Ui.Desktop.Windows.TextEdit;
using Omnius.Core;
using Omnius.Core.Avalonia;

namespace Omnius.Axus.Ui.Desktop.Internal;

public partial class Bootstrapper : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private AxusEnvironment? _axusEnvironment;
    private ServiceProvider? _serviceProvider;

    public static Bootstrapper Instance { get; } = new Bootstrapper();

    private const string UI_STATUS_FILE_NAME = "ui_status.json";

    private Bootstrapper()
    {
    }

    public async ValueTask BuildAsync(AxusEnvironment axusEnvironment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(axusEnvironment);

        _axusEnvironment = axusEnvironment;

        try
        {
            var bytesPool = BytesPool.Shared;

            var uiState = await UiStatus.LoadAsync(Path.Combine(_axusEnvironment.DatabaseDirectoryPath, UI_STATUS_FILE_NAME));

            var axusServiceProvider = await AxusServiceProvider.CreateAsync(axusEnvironment.ListenAddress, cancellationToken);
            var axusServiceMediator = new AxusServiceMediator(axusServiceProvider.GetService());
            var interactorProvider = await InteractorProvider.CreateAsync(_axusEnvironment.DatabaseDirectoryPath, axusServiceMediator, bytesPool, cancellationToken);

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(_axusEnvironment);
            serviceCollection.AddSingleton<IBytesPool>(bytesPool);
            serviceCollection.AddSingleton(uiState);
            serviceCollection.AddSingleton<IAxusServiceMediator>(axusServiceMediator);
            serviceCollection.AddSingleton<IInteractorProvider>(interactorProvider);

            serviceCollection.AddSingleton<IApplicationDispatcher, ApplicationDispatcher>();
            serviceCollection.AddSingleton<IMainWindowProvider, MainWindowProvider>();
            serviceCollection.AddSingleton<IClipboardService, ClipboardService>();
            serviceCollection.AddSingleton<IDialogService, DialogService>();

            serviceCollection.AddTransient<MainWindowModel>();
            serviceCollection.AddTransient<SettingsWindowModel>();
            serviceCollection.AddTransient<TextEditWindowModel>();
            serviceCollection.AddTransient<StatusViewViewModel>();
            serviceCollection.AddTransient<PeersViewViewModel>();
            serviceCollection.AddTransient<DownloadViewViewModel>();
            serviceCollection.AddTransient<UploadViewViewModel>();
            serviceCollection.AddTransient<SignaturesViewViewModel>();

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
        if (_axusEnvironment is null) return;
        if (_serviceProvider is null) return;

        var uiStatus = _serviceProvider.GetRequiredService<UiStatus>();
        await uiStatus.SaveAsync(Path.Combine(_axusEnvironment.DatabaseDirectoryPath, UI_STATUS_FILE_NAME));

        await _serviceProvider.GetRequiredService<IInteractorProvider>().DisposeAsync();
    }

    public ServiceProvider GetServiceProvider()
    {
        return _serviceProvider ?? throw new NullReferenceException();
    }
}
