using System.Reactive.Disposables;
using Avalonia.Controls;
using Omnius.Core;
using Omnius.Xeus.Intaractors;
using Omnius.Xeus.Intaractors.Models;
using Omnius.Xeus.Ui.Desktop.Configuration;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.Windows;

public class SettingsWindowViewModel : AsyncDisposableBase
{
    private readonly UiState _uiState;
    private readonly IFileDownloader _fileDownloader;

    private readonly CompositeDisposable _disposable = new();

    public SettingsWindowViewModel(UiState uiState, IFileDownloader fileDownloader)
    {
        _uiState = uiState;
        _fileDownloader = fileDownloader;

        this.DownloadDirectory = new ReactiveProperty<string>().AddTo(_disposable);

        this.OpenDownloadDirectoryCommand = new ReactiveCommand().AddTo(_disposable);
        this.OpenDownloadDirectoryCommand.Subscribe(() => this.OpenDownloadDirectory()).AddTo(_disposable);

        this.OkCommand = new ReactiveCommand().AddTo(_disposable);
        this.OkCommand.Subscribe((state) => this.Ok(state)).AddTo(_disposable);
        this.CancelCommand = new ReactiveCommand().AddTo(_disposable);
        this.CancelCommand.Subscribe((state) => this.Cancel(state)).AddTo(_disposable);
    }

    public async ValueTask InitializeAsync()
    {
        await this.LoadAsync();
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();
    }

    public ReactiveProperty<string> DownloadDirectory { get; }

    public ReactiveCommand OpenDownloadDirectoryCommand { get; }

    private async void OpenDownloadDirectory()
    {
    }

    public ReactiveCommand OkCommand { get; }

    public ReactiveCommand CancelCommand { get; }

    private async void Ok(object state)
    {
        var window = (Window)state;
        window.Close();

        await this.SaveAsync();
    }

    private async void Cancel(object state)
    {
        var window = (Window)state;
        window.Close();
    }

    private async ValueTask LoadAsync(CancellationToken cancellationToken = default)
    {
        var config = await _fileDownloader.GetConfigAsync();
        this.DownloadDirectory.Value = config?.DestinationDirectory ?? string.Empty;
    }

    private async ValueTask SaveAsync(CancellationToken cancellationToken = default)
    {
        var config = new FileDownloaderConfig(this.DownloadDirectory.Value);
        await _fileDownloader.SetConfigAsync(config);
    }
}
