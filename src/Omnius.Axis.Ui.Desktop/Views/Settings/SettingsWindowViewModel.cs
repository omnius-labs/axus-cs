using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Axis.Intaractors.Models;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Core;
using Reactive.Bindings;

namespace Omnius.Axis.Ui.Desktop.Views.Settings;

public abstract class SettingsWindowViewModelBase : AsyncDisposableBase
{
    public SettingsWindowStatus? Status { get; protected set; }

    public SignaturesControlViewModelBase? TrustedSignaturesControlViewModel { get; protected set; }

    public SignaturesControlViewModelBase? BlockedSignaturesControlViewModel { get; protected set; }

    public ReactiveProperty<string>? DownloadDirectory { get; protected set; }

    public AsyncReactiveCommand? EditDownloadDirectoryCommand { get; protected set; }

    public AsyncReactiveCommand? OkCommand { get; protected set; }

    public AsyncReactiveCommand? CancelCommand { get; protected set; }
}

public class SettingsWindowViewModel : SettingsWindowViewModelBase
{
    private readonly UiStatus _uiState;
    private readonly IIntaractorProvider _intaractorAdapter;

    private readonly CompositeDisposable _disposable = new();
    private readonly CompositeAsyncDisposable _asyncDisposable = new();

    public SettingsWindowViewModel(UiStatus uiState, IIntaractorProvider intaractorAdapter)
    {
        _uiState = uiState;
        _intaractorAdapter = intaractorAdapter;

        this.Status = _uiState.SettingsWindow ??= new SettingsWindowStatus();

        var serviceProvider = Bootstrapper.Instance.GetServiceProvider();

        this.TrustedSignaturesControlViewModel = serviceProvider.GetRequiredService<SignaturesControlViewModel>().AddTo(_asyncDisposable);

        this.BlockedSignaturesControlViewModel = serviceProvider.GetRequiredService<SignaturesControlViewModel>().AddTo(_asyncDisposable);

        this.DownloadDirectory = new ReactiveProperty<string>().AddTo(_disposable);

        this.EditDownloadDirectoryCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.EditDownloadDirectoryCommand.Subscribe(async () => await this.EditDownloadDirectoryAsync()).AddTo(_disposable);

        this.OkCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.OkCommand.Subscribe(async (state) => await this.OkAsync(state)).AddTo(_disposable);

        this.CancelCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.CancelCommand.Subscribe(async (state) => await this.CancelAsync(state)).AddTo(_disposable);

        this.Initialize();
    }

    private async void Initialize()
    {
        await this.LoadAsync();
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();
        await _asyncDisposable.DisposeAsync();
    }

    private async Task EditDownloadDirectoryAsync()
    {
    }

    private async Task OkAsync(object state)
    {
        var window = (Window)state;
        window.Close();

        await this.SaveAsync();
    }

    private async Task CancelAsync(object state)
    {
        var window = (Window)state;
        window.Close();
    }

    private async ValueTask LoadAsync(CancellationToken cancellationToken = default)
    {
        // Service
        var serviceAdapter = await _intaractorAdapter.GetServiceAdapterAsync(cancellationToken);
        var serviceConfig = await serviceAdapter.GetConfigAsync(cancellationToken);

        // ProfileSubscriber
        var profileSubscriber = await _intaractorAdapter.GetProfileSubscriberAsync(cancellationToken);
        var profileSubscriberConfig = await profileSubscriber.GetConfigAsync(cancellationToken);
        this.TrustedSignaturesControlViewModel!.SetSignatures(profileSubscriberConfig.TrustedSignatures);
        this.BlockedSignaturesControlViewModel!.SetSignatures(profileSubscriberConfig.BlockedSignatures);

        // FileDownloader
        var fileDownloader = await _intaractorAdapter.GetFileDownloaderAsync(cancellationToken);
        var fileDownloaderConfig = await fileDownloader.GetConfigAsync(cancellationToken);
        this.DownloadDirectory!.Value = fileDownloaderConfig?.DestinationDirectory ?? string.Empty;
    }

    private async ValueTask SaveAsync(CancellationToken cancellationToken = default)
    {
        // ProfileSubscriber
        var profileSubscriberConfig = new ProfileSubscriberConfig(this.TrustedSignaturesControlViewModel!.GetSignatures().ToArray(),
            this.BlockedSignaturesControlViewModel!.GetSignatures().ToArray(), 20, 1024);
        var profileSubscriber = await _intaractorAdapter.GetProfileSubscriberAsync(cancellationToken);
        await profileSubscriber.SetConfigAsync(profileSubscriberConfig, cancellationToken);

        // FileDownloader
        var fileDownloaderConfig = new FileDownloaderConfig(this.DownloadDirectory!.Value);
        var fileDownloader = await _intaractorAdapter.GetFileDownloaderAsync(cancellationToken);
        await fileDownloader.SetConfigAsync(fileDownloaderConfig, cancellationToken);
    }
}
