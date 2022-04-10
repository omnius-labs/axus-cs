using Microsoft.Extensions.DependencyInjection;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Core;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axis.Ui.Desktop.Windows.Main;

public abstract class MainWindowModelBase : AsyncDisposableBase
{
    public MainWindowStatus? Status { get; protected set; }

    public StatusViewViewModel? StatusViewViewModel { get; protected set; }

    public PeersViewViewModelBase? PeersViewViewModel { get; protected set; }

    public DownloadViewViewModel? DownloadViewViewModel { get; protected set; }

    public UploadViewViewModel? UploadViewViewModel { get; protected set; }

    public AsyncReactiveCommand? SettingsCommand { get; protected set; }
}

public class MainWindowModel : MainWindowModelBase
{
    private readonly UiStatus _uiState;
    private readonly IDialogService _dialogService;

    private readonly CompositeDisposable _disposable = new();
    private readonly CompositeAsyncDisposable _asyncDisposable = new();

    public MainWindowModel(UiStatus uiState, IDialogService dialogService)
    {
        _uiState = uiState;
        _dialogService = dialogService;

        this.Status = _uiState.MainWindow ??= new MainWindowStatus();

        var serviceProvider = Bootstrapper.Instance.GetServiceProvider();

        this.StatusViewViewModel = serviceProvider.GetRequiredService<StatusViewViewModel>();
        this.PeersViewViewModel = serviceProvider.GetRequiredService<PeersViewViewModel>();
        this.DownloadViewViewModel = serviceProvider.GetRequiredService<DownloadViewViewModel>();
        this.UploadViewViewModel = serviceProvider.GetRequiredService<UploadViewViewModel>();

        this.SettingsCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.SettingsCommand.Subscribe(async () => await this.SettingsAsync()).AddTo(_disposable);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();

        await this.StatusViewViewModel!.DisposeAsync();
        await this.PeersViewViewModel!.DisposeAsync();
        await this.DownloadViewViewModel!.DisposeAsync();
        await this.UploadViewViewModel!.DisposeAsync();
    }

    private async Task SettingsAsync()
    {
        await _dialogService.ShowSettingsWindowAsync();
    }
}
