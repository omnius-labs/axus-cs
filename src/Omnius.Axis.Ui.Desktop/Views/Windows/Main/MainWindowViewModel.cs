using System.Reactive.Disposables;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Axis.Ui.Desktop.Controls;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Core;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axis.Ui.Desktop.Windows;

public class MainWindowViewModel : AsyncDisposableBase
{
    private readonly UiStatus _uiState;
    private readonly IDialogService _dialogService;

    private readonly CompositeDisposable _disposable = new();

    public MainWindowViewModel(UiStatus uiState, IDialogService dialogService,
        StatusControlViewModel statusControlViewModel, PeersControlViewModel peersControlViewModel,
        DownloadControlViewModel downloadControlViewModel, UploadControlViewModel uploadControlViewModel)
    {
        _uiState = uiState;
        _dialogService = dialogService;

        this.StatusControlViewModel = statusControlViewModel;
        this.PeersControlViewModel = peersControlViewModel;
        this.DownloadControlViewModel = downloadControlViewModel;
        this.UploadControlViewModel = uploadControlViewModel;

        this.SettingsCommand = new ReactiveCommand().AddTo(_disposable);
        this.SettingsCommand.Subscribe(() => this.Settings()).AddTo(_disposable);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();
    }

    public MainWindowStatus Status => _uiState.MainWindow ??= new MainWindowStatus();

    public StatusControlViewModel StatusControlViewModel { get; }

    public PeersControlViewModel PeersControlViewModel { get; }

    public DownloadControlViewModel DownloadControlViewModel { get; }

    public UploadControlViewModel UploadControlViewModel { get; }

    public ReactiveCommand SettingsCommand { get; }

    private async void Settings()
    {
        await _dialogService.ShowSettingsWindowAsync();
    }
}
