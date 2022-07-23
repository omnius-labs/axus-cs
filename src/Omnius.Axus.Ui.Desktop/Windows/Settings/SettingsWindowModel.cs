using Avalonia.Controls;
using Omnius.Axus.Interactors;
using Omnius.Axus.Interactors.Models;
using Omnius.Axus.Models;
using Omnius.Axus.Ui.Desktop.Internal;
using Omnius.Axus.Ui.Desktop.Models;
using Omnius.Core;
using Omnius.Core.Net;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axus.Ui.Desktop.Windows.Settings;

public abstract class SettingsWindowModelBase : AsyncDisposableBase
{
    public abstract ValueTask InitializeAsync(CancellationToken cancellationToken = default);

    public SettingsWindowStatus? Status { get; protected set; }

    public ReactiveProperty<string>? NetworkBandwidth { get; protected set; }

    public ReactiveProperty<bool>? I2pConnectorIsEnabled { get; protected set; }

    public ReactiveProperty<string>? I2pConnectorSamBridgeAddress { get; protected set; }

    public ReactiveProperty<bool>? I2pAccepterIsEnabled { get; protected set; }

    public ReactiveProperty<string>? I2pAccepterSamBridgeAddress { get; protected set; }

    public ReactiveProperty<bool>? TcpConnectorIsEnabled { get; protected set; }

    public IEnumerable<TcpProxyType>? TcpConnectorTcpProxyTypes { get; protected set; }

    public ReactiveProperty<TcpProxyType>? TcpConnectorSelectedProxyType { get; protected set; }

    public ReactiveProperty<string>? TcpConnectorProxyAddress { get; protected set; }

    public ReactiveProperty<bool>? TcpAccepterIsEnabled { get; protected set; }

    public ReactiveProperty<bool>? TcpAccepterUseUpnp { get; protected set; }

    public ReactiveProperty<string>? TcpAccepterListenAddress { get; protected set; }

    public ReactiveProperty<string>? FileDownloadDirectory { get; protected set; }

    public AsyncReactiveCommand? OpenFileDownloadDirectoryCommand { get; protected set; }

    public AsyncReactiveCommand? OkCommand { get; protected set; }

    public AsyncReactiveCommand? CancelCommand { get; protected set; }
}

public class SettingsWindowModel : SettingsWindowModelBase
{
    private readonly UiStatus _uiState;
    private readonly IAxusServiceMediator _axusServiceMediator;
    private readonly IInteractorProvider _interactorProvider;

    private readonly CompositeDisposable _disposable = new();
    private readonly CompositeAsyncDisposable _asyncDisposable = new();

    public SettingsWindowModel(UiStatus uiState, IAxusServiceMediator axusServiceMediator, IInteractorProvider interactorProvider)
    {
        _uiState = uiState;
        _axusServiceMediator = axusServiceMediator;
        _interactorProvider = interactorProvider;

        this.Status = _uiState.SettingsWindow ??= new SettingsWindowStatus();

        this.NetworkBandwidth = new ReactiveProperty<string>().AddTo(_disposable);
        this.I2pAccepterIsEnabled = new ReactiveProperty<bool>().AddTo(_disposable);
        this.I2pAccepterSamBridgeAddress = new ReactiveProperty<string>().AddTo(_disposable);
        this.I2pConnectorIsEnabled = new ReactiveProperty<bool>().AddTo(_disposable);
        this.I2pConnectorSamBridgeAddress = new ReactiveProperty<string>().AddTo(_disposable);
        this.TcpConnectorIsEnabled = new ReactiveProperty<bool>().AddTo(_disposable);
        this.TcpConnectorTcpProxyTypes = Enum.GetValues<TcpProxyType>();
        this.TcpConnectorSelectedProxyType = new ReactiveProperty<TcpProxyType>().AddTo(_disposable);
        this.TcpConnectorProxyAddress = new ReactiveProperty<string>().AddTo(_disposable);
        this.TcpAccepterIsEnabled = new ReactiveProperty<bool>().AddTo(_disposable);
        this.TcpAccepterUseUpnp = new ReactiveProperty<bool>().AddTo(_disposable);
        this.TcpAccepterListenAddress = new ReactiveProperty<string>().AddTo(_disposable);
        this.FileDownloadDirectory = new ReactiveProperty<string>().AddTo(_disposable);
        this.OpenFileDownloadDirectoryCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.OpenFileDownloadDirectoryCommand.Subscribe(async () => await this.OpenDownloadDirectoryAsync()).AddTo(_disposable);
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

    private async ValueTask LoadAsync(CancellationToken cancellationToken = default)
    {
        var fileDownloader = _interactorProvider.GetFileDownloader();

        var serviceConfig = await _axusServiceMediator.GetConfigAsync(cancellationToken);
        var fileDownloaderConfig = await fileDownloader.GetConfigAsync(cancellationToken);

        this.NetworkBandwidth!.Value = (((serviceConfig.Bandwidth?.MaxReceiveBytesPerSeconds ?? 0) + (serviceConfig.Bandwidth?.MaxSendBytesPerSeconds ?? 0)) / 2).ToString();
        this.I2pConnectorIsEnabled!.Value = serviceConfig.I2pConnector?.IsEnabled ?? false;
        this.I2pConnectorSamBridgeAddress!.Value = serviceConfig.I2pConnector?.SamBridgeAddress?.ToString() ?? string.Empty;
        this.I2pAccepterIsEnabled!.Value = serviceConfig.I2pAccepter?.IsEnabled ?? false;
        this.I2pAccepterSamBridgeAddress!.Value = serviceConfig.I2pAccepter?.SamBridgeAddress?.ToString() ?? string.Empty;
        this.TcpConnectorIsEnabled!.Value = serviceConfig.TcpConnector?.IsEnabled ?? false;
        this.TcpConnectorSelectedProxyType!.Value = serviceConfig.TcpConnector?.Proxy?.Type ?? TcpProxyType.None;
        this.TcpConnectorProxyAddress!.Value = serviceConfig.TcpConnector?.Proxy?.Address?.ToString() ?? string.Empty;
        this.TcpAccepterIsEnabled!.Value = serviceConfig.TcpAccepter?.IsEnabled ?? false;
        this.TcpAccepterUseUpnp!.Value = serviceConfig.TcpAccepter?.UseUpnp ?? false;
        this.TcpAccepterListenAddress!.Value = serviceConfig.TcpAccepter?.ListenAddress?.ToString() ?? string.Empty;
        this.FileDownloadDirectory!.Value = fileDownloaderConfig?.DestinationDirectory ?? string.Empty;
    }

    private async ValueTask SaveAsync(CancellationToken cancellationToken = default)
    {
        var serviceConfig = new ServiceConfig(
            new BandwidthConfig(
                maxSendBytesPerSeconds: int.Parse(this.NetworkBandwidth!.Value ?? "0"),
                maxReceiveBytesPerSeconds: int.Parse(this.NetworkBandwidth!.Value ?? "0")),
            new I2pConnectorConfig(
                isEnabled: this.I2pConnectorIsEnabled!.Value,
                samBridgeAddress: OmniAddress.Parse(this.I2pConnectorSamBridgeAddress!.Value)
            ),
            new I2pAccepterConfig(
                isEnabled: this.I2pAccepterIsEnabled!.Value,
                samBridgeAddress: OmniAddress.Parse(this.I2pAccepterSamBridgeAddress!.Value)
            ),
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
        var fileDownloaderConfig = new FileDownloaderConfig(this.FileDownloadDirectory!.Value);

        var fileDownloader = _interactorProvider.GetFileDownloader();

        await _axusServiceMediator.SetConfigAsync(serviceConfig, cancellationToken);
        await fileDownloader.SetConfigAsync(fileDownloaderConfig, cancellationToken);
    }
}
