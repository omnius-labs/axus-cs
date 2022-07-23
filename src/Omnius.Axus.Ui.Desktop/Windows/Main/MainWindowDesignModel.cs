using Omnius.Axus.Ui.Desktop.Models;
using Omnius.Axus.Ui.Desktop.Windows.Settings;
using Omnius.Core;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axus.Ui.Desktop.Windows.Main;

public class MainWindowDesignModel : MainWindowModelBase
{
    private readonly CompositeDisposable _disposable = new();
    private readonly CompositeAsyncDisposable _asyncDisposable = new();

    public MainWindowDesignModel()
    {
        this.Status = new MainWindowStatus();

        this.PeersViewViewModel = new PeersViewDesignViewModel();

        this.SettingsCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.SettingsCommand.Subscribe(async () => await this.SettingsAsync()).AddTo(_disposable);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();

        await this.PeersViewViewModel!.DisposeAsync();
    }

    private async Task SettingsAsync()
    {
        var window = new SettingsWindow();
        window.ViewModel = new SettingsWindowDesignModel();
        await window.ShowDialog(App.Current!.MainWindow);
    }
}
