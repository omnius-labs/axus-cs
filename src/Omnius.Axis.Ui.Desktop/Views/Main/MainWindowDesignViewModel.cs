using System.Reactive.Disposables;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Axis.Ui.Desktop.Views.Settings;
using Omnius.Core;
using Reactive.Bindings;

namespace Omnius.Axis.Ui.Desktop.Views.Main;

public class MainWindowDesignViewModel : MainWindowViewModelBase
{
    private readonly CompositeDisposable _disposable = new();

    public MainWindowDesignViewModel()
    {
        this.Status = new MainWindowStatus();

        this.SettingsCommand = new AsyncReactiveCommand().ToAdd(_disposable);
        this.SettingsCommand.Subscribe(() => this.SettingsAsync()).ToAdd(_disposable);
    }

    protected override void OnDispose(bool disposing)
    {
        _disposable.Dispose();
    }

    private async Task SettingsAsync()
    {
        var window = new SettingsWindow();
        window.Show();
    }
}
