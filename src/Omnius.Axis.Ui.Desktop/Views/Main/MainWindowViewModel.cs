using Microsoft.Extensions.DependencyInjection;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Core;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axis.Ui.Desktop.Views.Main;

public abstract class MainWindowViewModelBase : AsyncDisposableBase
{
    public MainWindowStatus? Status { get; protected set; }

    public StatusControlViewModel? StatusControlViewModel { get; protected set; }

    public PeersControlViewModelBase? PeersControlViewModel { get; protected set; }

    public DownloadControlViewModel? DownloadControlViewModel { get; protected set; }

    public UploadControlViewModel? UploadControlViewModel { get; protected set; }

    public AsyncReactiveCommand? SettingsCommand { get; protected set; }
}

public class MainWindowViewModel : MainWindowViewModelBase
{
    private readonly UiStatus _uiState;
    private readonly IDialogService _dialogService;

    private readonly CompositeDisposable _disposable = new();
    private readonly CompositeAsyncDisposable _asyncDisposable = new();

    public MainWindowViewModel(UiStatus uiState, IDialogService dialogService)
    {
        _uiState = uiState;
        _dialogService = dialogService;

        this.Status = _uiState.MainWindow ??= new MainWindowStatus();

        var serviceProvider = Bootstrapper.Instance.GetServiceProvider();

        this.StatusControlViewModel = serviceProvider.GetRequiredService<StatusControlViewModel>();
        this.PeersControlViewModel = serviceProvider.GetRequiredService<PeersControlViewModel>();
        this.DownloadControlViewModel = serviceProvider.GetRequiredService<DownloadControlViewModel>();
        this.UploadControlViewModel = serviceProvider.GetRequiredService<UploadControlViewModel>();

        this.SettingsCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.SettingsCommand.Subscribe(async () => await this.SettingsAsync()).AddTo(_disposable);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();

        await this.StatusControlViewModel!.DisposeAsync();
        await this.PeersControlViewModel!.DisposeAsync();
        await this.DownloadControlViewModel!.DisposeAsync();
        await this.UploadControlViewModel!.DisposeAsync();
    }

    private async Task SettingsAsync()
    {
        await _dialogService.ShowSettingsWindowAsync();
    }
}
