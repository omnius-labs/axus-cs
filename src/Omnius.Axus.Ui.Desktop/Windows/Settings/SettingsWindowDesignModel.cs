using Avalonia.Controls;
using Omnius.Axus.Ui.Desktop.Models;
using Omnius.Core;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axus.Ui.Desktop.Windows.Settings;

public class SettingsWindowDesignModel : SettingsWindowModelBase
{
    private readonly CompositeDisposable _disposable = new();

    public SettingsWindowDesignModel()
    {
        this.Status = new SettingsWindowStatus();

        this.FileDownloadDirectory = new ReactiveProperty<string>().AddTo(_disposable);

        this.OpenFileDownloadDirectoryCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.OpenFileDownloadDirectoryCommand.Subscribe(async () => await this.EditDownloadDirectoryAsync()).AddTo(_disposable);

        this.OkCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.OkCommand.Subscribe(async (state) => await this.OkAsync(state)).AddTo(_disposable);

        this.CancelCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.CancelCommand.Subscribe(async (state) => await this.CancelAsync(state)).AddTo(_disposable);
    }

    public override async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();
    }

    private async Task EditDownloadDirectoryAsync()
    {
    }

    private async Task OkAsync(object state)
    {
        var window = (Window)state;
        window.Close();
    }

    private async Task CancelAsync(object state)
    {
        var window = (Window)state;
        window.Close();
    }
}
