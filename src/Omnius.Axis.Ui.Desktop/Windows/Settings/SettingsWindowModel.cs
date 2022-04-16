using Avalonia.Controls;
using Omnius.Axis.Intaractors.Models;
using Omnius.Axis.Models;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Axis.Ui.Desktop.Models;
using Omnius.Core;
using Omnius.Core.Net;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axis.Ui.Desktop.Windows.Settings;

public abstract class SettingsWindowModelBase : AsyncDisposableBase
{
    public abstract ValueTask InitializeAsync(CancellationToken cancellationToken = default);

    public SettingsWindowStatus? Status { get; protected set; }

    // public ReactiveProperty<string>? ProfileSignature { get; protected set; }

    // public SignaturesViewViewModelBase? TrustedSignaturesViewViewModel { get; protected set; }

    // public SignaturesViewViewModelBase? BlockedSignaturesViewViewModel { get; protected set; }

    public ReactiveProperty<string>? FileDownloadDirectory { get; protected set; }

    public AsyncReactiveCommand? OpenFileDownloadDirectoryCommand { get; protected set; }

    public ReactiveProperty<string>? ServiceBandwidth { get; protected set; }

    public ReactiveProperty<bool>? TcpConnectorIsEnabled { get; protected set; }

    public IEnumerable<TcpProxyType>? TcpConnectorTcpProxyTypes { get; protected set; }

    public ReactiveProperty<TcpProxyType>? TcpConnectorSelectedProxyType { get; protected set; }

    public ReactiveProperty<string>? TcpConnectorProxyAddress { get; protected set; }

    public ReactiveProperty<bool>? TcpAccepterIsEnabled { get; protected set; }

    public ReactiveProperty<bool>? TcpAccepterUseUpnp { get; protected set; }

    public ReactiveProperty<string>? TcpAccepterListenAddress { get; protected set; }

    public AsyncReactiveCommand? OkCommand { get; protected set; }

    public AsyncReactiveCommand? CancelCommand { get; protected set; }
}

public class SettingsWindowModel : SettingsWindowModelBase
{
    private readonly UiStatus _uiState;
    private readonly IIntaractorProvider _intaractorAdapter;

    private readonly CompositeDisposable _disposable = new();
    private readonly CompositeAsyncDisposable _asyncDisposable = new();

    public SettingsWindowModel(UiStatus uiState, IIntaractorProvider intaractorAdapter)
    {
        _uiState = uiState;
        _intaractorAdapter = intaractorAdapter;

        this.Status = _uiState.SettingsWindow ??= new SettingsWindowStatus();

        this.FileDownloadDirectory = new ReactiveProperty<string>().AddTo(_disposable);

        this.OpenFileDownloadDirectoryCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.OpenFileDownloadDirectoryCommand.Subscribe(async () => await this.OpenDownloadDirectoryAsync()).AddTo(_disposable);

        this.ServiceBandwidth = new ReactiveProperty<string>().AddTo(_disposable);

        this.TcpConnectorIsEnabled = new ReactiveProperty<bool>().AddTo(_disposable);

        this.TcpConnectorTcpProxyTypes = Enum.GetValues<TcpProxyType>();

        this.TcpConnectorSelectedProxyType = new ReactiveProperty<TcpProxyType>().AddTo(_disposable);

        this.TcpConnectorProxyAddress = new ReactiveProperty<string>().AddTo(_disposable);

        this.TcpAccepterIsEnabled = new ReactiveProperty<bool>().AddTo(_disposable);

        this.TcpAccepterUseUpnp = new ReactiveProperty<bool>().AddTo(_disposable);

        this.TcpAccepterListenAddress = new ReactiveProperty<string>().AddTo(_disposable);

        this.OkCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.OkCommand.Subscribe(async (state) => await this.OkAsync(state)).AddTo(_disposable);

        this.CancelCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.CancelCommand.Subscribe(async (state) => await this.CancelAsync(state)).AddTo(_disposable);
    }

    public override async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        await this.LoadAsync();
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();
        await _asyncDisposable.DisposeAsync();
    }

    private async Task OpenDownloadDirectoryAsync()
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

    // public ReactiveProperty<string>? FileDownloadDirectory { get; protected set; }
    // public AsyncReactiveCommand? OpenDownloadDirectoryCommand { get; protected set; }
    // public ReactiveProperty<string>? ServiceBandwidth { get; protected set; }
    // public ReactiveProperty<bool>? TcpConnectorIsEnabled { get; protected set; }
    // public IEnumerable<TcpProxyType>? TcpConnectorTcpProxyTypes { get; protected set; }
    // public ReactiveProperty<TcpProxyType>? TcpConnectorSelectedProxyType { get; protected set; }
    // public ReactiveProperty<string>? TcpConnectorProxyAddress { get; protected set; }
    // public ReactiveProperty<bool>? TcpAccepterIsEnabled { get; protected set; }
    // public ReactiveProperty<bool>? TcpAccepterUseUpnp { get; protected set; }
    // public ReactiveProperty<string>? TcpAccepterListenAddress { get; protected set; }

    private async ValueTask LoadAsync(CancellationToken cancellationToken = default)
    {
        var fileDownloader = await _intaractorAdapter.GetFileDownloaderAsync(cancellationToken);
        var fileDownloaderConfig = await fileDownloader.GetConfigAsync(cancellationToken);
        this.FileDownloadDirectory!.Value = fileDownloaderConfig?.DestinationDirectory ?? string.Empty;

        var serviceController = await _intaractorAdapter.GetServiceControllerAsync(cancellationToken);
        var serviceConfig = await serviceController.GetConfigAsync(cancellationToken);
        this.ServiceBandwidth!.Value = ((serviceConfig.Bandwidth?.MaxReceiveBytesPerSeconds ?? 0 + serviceConfig.Bandwidth?.MaxSendBytesPerSeconds ?? 0) / 2).ToString();
        this.TcpConnectorIsEnabled!.Value = serviceConfig.TcpConnector?.IsEnabled ?? false;
        this.TcpConnectorSelectedProxyType!.Value = serviceConfig.TcpConnector?.Proxy?.Type ?? TcpProxyType.None;
        this.TcpConnectorProxyAddress!.Value = serviceConfig.TcpConnector?.Proxy?.Address?.ToString() ?? string.Empty;
        this.TcpAccepterIsEnabled!.Value = serviceConfig.TcpAccepter?.IsEnabled ?? false;
        this.TcpAccepterUseUpnp!.Value = serviceConfig.TcpAccepter?.UseUpnp ?? false;
        this.TcpAccepterListenAddress!.Value = serviceConfig.TcpAccepter?.ListenAddress?.ToString() ?? string.Empty;
    }

    private async ValueTask SaveAsync(CancellationToken cancellationToken = default)
    {
        var fileDownloaderConfig = new FileDownloaderConfig(this.FileDownloadDirectory!.Value);
        var fileDownloader = await _intaractorAdapter.GetFileDownloaderAsync(cancellationToken);
        await fileDownloader.SetConfigAsync(fileDownloaderConfig, cancellationToken);

        var serviceConfig = new ServiceConfig(
            new BandwidthConfig(
                maxSendBytesPerSeconds: int.Parse(this.ServiceBandwidth!.Value ?? "0"),
                maxReceiveBytesPerSeconds: int.Parse(this.ServiceBandwidth!.Value ?? "0")),
            new TcpConnectorConfig(
                isEnabled: this.TcpConnectorIsEnabled!.Value,
                proxy: new TcpProxyConfig(
                    type: TcpConnectorSelectedProxyType!.Value,
                    address: OmniAddress.Parse(TcpConnectorProxyAddress!.Value)
                )
            ),
            new TcpAccepterConfig(
                isEnabled: this.TcpAccepterIsEnabled!.Value,
                useUpnp: this.TcpAccepterUseUpnp!.Value,
                listenAddress: OmniAddress.Parse(this.TcpAccepterListenAddress!.Value)
            ));
        var serviceController = await _intaractorAdapter.GetServiceControllerAsync(cancellationToken);
        await serviceController.SetConfigAsync(serviceConfig, cancellationToken);
    }
}
