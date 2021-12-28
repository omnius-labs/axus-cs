using System.Reactive.Disposables;
using Avalonia.Controls;
using Omnius.Axis.Intaractors.Models;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Core;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axis.Ui.Desktop.Windows;

public class SettingsWindowViewModel : AsyncDisposableBase
{
    private readonly UiStatus _uiState;
    private readonly IIntaractorProvider _intaractorAdapter;

    private readonly CompositeDisposable _disposable = new();

    public SettingsWindowViewModel(UiStatus uiState, IIntaractorProvider intaractorAdapter)
    {
        _uiState = uiState;
        _intaractorAdapter = intaractorAdapter;

        this.DownloadDirectory = new ReactiveProperty<string>().AddTo(_disposable);

        this.OpenDownloadDirectoryCommand = new ReactiveCommand().AddTo(_disposable);
        this.OpenDownloadDirectoryCommand.Subscribe(() => this.OpenDownloadDirectory()).AddTo(_disposable);

        this.OkCommand = new ReactiveCommand().AddTo(_disposable);
        this.OkCommand.Subscribe((state) => this.Ok(state)).AddTo(_disposable);
        this.CancelCommand = new ReactiveCommand().AddTo(_disposable);
        this.CancelCommand.Subscribe((state) => this.Cancel(state)).AddTo(_disposable);

        this.Initialize();
    }

    private async void Initialize()
    {
        await this.LoadAsync();
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();
    }

    public SettingsWindowStatus Status => _uiState.SettingsWindow ??= new SettingsWindowStatus();

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
        var fileDownloader = await _intaractorAdapter.GetFileDownloaderAsync(cancellationToken);
        var config = await fileDownloader.GetConfigAsync(cancellationToken);
        this.DownloadDirectory.Value = config?.DestinationDirectory ?? string.Empty;
    }

    private async ValueTask SaveAsync(CancellationToken cancellationToken = default)
    {
        var config = new FileDownloaderConfig(this.DownloadDirectory.Value);
        var fileDownloader = await _intaractorAdapter.GetFileDownloaderAsync(cancellationToken);
        await fileDownloader.SetConfigAsync(config, cancellationToken);
    }
}
