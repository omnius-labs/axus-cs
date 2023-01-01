using Microsoft.Extensions.DependencyInjection;
using Omnius.Axus.Ui.Desktop.Internal;
using Omnius.Axus.Ui.Desktop.Configuration;
using Omnius.Core;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axus.Ui.Desktop.Windows.Main;

public abstract class MainWindowModelBase : AsyncDisposableBase
{
    public MainWindowStatus? Status { get; protected set; }
    public StatusViewModel? StatusViewModel { get; protected set; }
    public PeersViewModelBase? PeersViewModel { get; protected set; }
    public DownloadViewModel? DownloadViewModel { get; protected set; }
    public UploadViewModel? UploadViewModel { get; protected set; }
    public AsyncReactiveCommand? SettingsCommand { get; protected set; }
}

public class MainWindowModel : MainWindowModelBase
{
    private readonly UiStatus _uiState;
    private readonly IDialogService _dialogService;

    private readonly CompositeDisposable _disposable = new();

    public MainWindowModel(UiStatus uiState, IDialogService dialogService)
    {
        _uiState = uiState;
        _dialogService = dialogService;

        this.Status = _uiState.MainWindow ??= new MainWindowStatus();

        var serviceProvider = Bootstrapper.Instance.GetServiceProvider();

        this.StatusViewModel = serviceProvider.GetRequiredService<StatusViewModel>();
        this.PeersViewModel = serviceProvider.GetRequiredService<PeersViewModel>();
        this.DownloadViewModel = serviceProvider.GetRequiredService<DownloadViewModel>();
        this.UploadViewModel = serviceProvider.GetRequiredService<UploadViewModel>();
        this.SettingsCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.SettingsCommand.Subscribe(async () => await this.SettingsAsync()).AddTo(_disposable);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();

        await this.StatusViewModel!.DisposeAsync();
        await this.PeersViewModel!.DisposeAsync();
        await this.DownloadViewModel!.DisposeAsync();
        await this.UploadViewModel!.DisposeAsync();
    }

    private async Task SettingsAsync()
    {
        await _dialogService.ShowSettingsWindowAsync();
    }
}
