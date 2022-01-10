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

public abstract class SettingsWindowViewModelBase : DisposableBase
{
    public SettingsWindowStatus? Status { get; protected set; }

    public ISignaturesControlViewModel? TrustedSignaturesControlViewModel { get; protected set; }

    public ISignaturesControlViewModel? BlockedSignaturesControlViewModel { get; protected set; }

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

    public SettingsWindowViewModel(UiStatus uiState, IIntaractorProvider intaractorAdapter)
    {
        _uiState = uiState;
        _intaractorAdapter = intaractorAdapter;

        var serviceProvider = Bootstrapper.Instance.GetServiceProvider();

        this.TrustedSignaturesControlViewModel = serviceProvider.GetRequiredService<SignaturesControlViewModel>();

        this.BlockedSignaturesControlViewModel = serviceProvider.GetRequiredService<SignaturesControlViewModel>();

        this.DownloadDirectory = new ReactiveProperty<string>().AddTo(_disposable);

        this.EditDownloadDirectoryCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.EditDownloadDirectoryCommand.Subscribe(() => this.EditDownloadDirectoryAsync()).AddTo(_disposable);

        this.OkCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.OkCommand.Subscribe((state) => this.OkAsync(state)).AddTo(_disposable);

        this.CancelCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.CancelCommand.Subscribe((state) => this.CancelAsync(state)).AddTo(_disposable);

        this.Initialize();
    }

    private async void Initialize()
    {
        await this.LoadAsync();
    }

    protected override void OnDispose(bool disposing)
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

        await this.SaveAsync();
    }

    private async Task CancelAsync(object state)
    {
        var window = (Window)state;
        window.Close();
    }

    private async ValueTask LoadAsync(CancellationToken cancellationToken = default)
    {
        var profileSubscriber = await _intaractorAdapter.GetProfileSubscriberAsync(cancellationToken);
        var profileSubscriberConfig = await profileSubscriber.GetConfigAsync(cancellationToken);
        this.TrustedSignaturesControlViewModel.Signatures.AddRange(profileSubscriberConfig.TrustedSignatures);
        this.BlockedSignaturesControlViewModel.Signatures.AddRange(profileSubscriberConfig.BlockedSignatures);

        var fileDownloader = await _intaractorAdapter.GetFileDownloaderAsync(cancellationToken);
        var fileDownloaderConfig = await fileDownloader.GetConfigAsync(cancellationToken);
        this.DownloadDirectory.Value = fileDownloaderConfig?.DestinationDirectory ?? string.Empty;
    }

    private async ValueTask SaveAsync(CancellationToken cancellationToken = default)
    {
        var profileSubscriberConfig = new ProfileSubscriberConfig(this.TrustedSignaturesControlViewModel.Signatures.ToArray(), this.BlockedSignaturesControlViewModel.Signatures.ToArray(), 20, 1024);
        var profileSubscriber = await _intaractorAdapter.GetProfileSubscriberAsync(cancellationToken);
        await profileSubscriber.SetConfigAsync(profileSubscriberConfig, cancellationToken);

        var fileDownloaderConfig = new FileDownloaderConfig(this.DownloadDirectory.Value);
        var fileDownloader = await _intaractorAdapter.GetFileDownloaderAsync(cancellationToken);
        await fileDownloader.SetConfigAsync(fileDownloaderConfig, cancellationToken);
    }
}
