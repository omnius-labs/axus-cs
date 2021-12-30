using System.Reactive.Disposables;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Axis.Intaractors.Models;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Core;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axis.Ui.Desktop.Views.Settings;

public interface ISettingsWindowViewModel
{
    SettingsWindowStatus Status { get; }

    ISignaturesControlViewModel TrustedSignaturesControlViewModel { get; }

    ISignaturesControlViewModel BlockedSignaturesControlViewModel { get; }

    ReactiveProperty<string> DownloadDirectory { get; }

    ReactiveCommand EditDownloadDirectoryCommand { get; }

    ReactiveCommand OkCommand { get; }

    ReactiveCommand CancelCommand { get; }
}

public class SettingsWindowViewModel : AsyncDisposableBase, ISettingsWindowViewModel
{
    private readonly UiStatus _uiState;
    private readonly IIntaractorProvider _intaractorAdapter;

    private readonly CompositeDisposable _disposable = new();

    public SettingsWindowViewModel(UiStatus uiState, IIntaractorProvider intaractorAdapter)
    {
        _uiState = uiState;
        _intaractorAdapter = intaractorAdapter;

        var serviceProvider = Bootstrapper.Instance.GetServiceProvider();

        this.TrustedSignaturesControlViewModel = serviceProvider.GetRequiredService<SignaturesControlViewModel>();

        this.BlockedSignaturesControlViewModel = serviceProvider.GetRequiredService<SignaturesControlViewModel>();

        this.DownloadDirectory = new ReactiveProperty<string>().AddTo(_disposable);

        this.EditDownloadDirectoryCommand = new ReactiveCommand().AddTo(_disposable);
        this.EditDownloadDirectoryCommand.Subscribe(() => this.OpenDownloadDirectory()).AddTo(_disposable);

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

    public ISignaturesControlViewModel TrustedSignaturesControlViewModel { get; }

    public ISignaturesControlViewModel BlockedSignaturesControlViewModel { get; }

    public ReactiveProperty<string> DownloadDirectory { get; }

    public ReactiveCommand EditDownloadDirectoryCommand { get; }

    public ReactiveCommand OkCommand { get; }

    public ReactiveCommand CancelCommand { get; }

    private async void OpenDownloadDirectory()
    {
    }

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
