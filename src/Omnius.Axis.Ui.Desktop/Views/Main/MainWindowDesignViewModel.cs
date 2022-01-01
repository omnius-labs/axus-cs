using System.Reactive.Disposables;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Axis.Ui.Desktop.Views.Settings;
using Omnius.Core;
using Reactive.Bindings;

namespace Omnius.Axis.Ui.Desktop.Views.Main;

public class MainWindowDesignViewModel : AsyncDisposableBase, IMainWindowViewModel
{
    private readonly CompositeDisposable _disposable = new();

    public MainWindowDesignViewModel()
    {
        this.Status = new MainWindowStatus();
        this.SettingsCommand = new ReactiveCommand().ToAdd(_disposable);
        this.SettingsCommand.Subscribe(() => this.Settings()).ToAdd(_disposable);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();
    }

    private void Settings()
    {
        var window = new SettingsWindow();
        window.Show();
    }

    public MainWindowStatus Status { get; private set; }

    public ReactiveCommand SettingsCommand { get; private set; }

    public StatusControlViewModel StatusControlViewModel { get; private set; } = null!;

    public PeersControlViewModel PeersControlViewModel { get; private set; } = null!;

    public DownloadControlViewModel DownloadControlViewModel { get; private set; } = null!;

    public UploadControlViewModel UploadControlViewModel { get; private set; } = null!;
}
