using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Axus.Interactors;
using Omnius.Axus.Interactors.Models;
using Omnius.Axus.Remoting;
using Omnius.Axus.Ui.Desktop.Internal;
using Omnius.Axus.Ui.Desktop.Configuration;
using Omnius.Core;
using Omnius.Core.Cryptography;
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
    public ReactiveProperty<OmniDigitalSignature>? ProfileDigitalSignature { get; protected set; }
    public AsyncReactiveCommand? NewProfileSignatureCommand { get; protected set; }
    public SignaturesViewModelBase? TrustedSignaturesViewModel { get; protected set; }
    public SignaturesViewModelBase? BlockedSignaturesViewModel { get; protected set; }
    public ReactiveProperty<string>? FileDownloadDirectory { get; protected set; }
    public AsyncReactiveCommand? OpenFileDownloadDirectoryCommand { get; protected set; }
    public AsyncReactiveCommand? OkCommand { get; protected set; }
    public AsyncReactiveCommand? CancelCommand { get; protected set; }
}

public class SettingsWindowModel : SettingsWindowModelBase
{
    private readonly UiStatus _uiState;
    private readonly IAxusServiceMediator _serviceMediator;
    private readonly IInteractorProvider _interactorProvider;
    private readonly IDialogService _dialogService;

    private readonly CompositeDisposable _disposable = new();

    public SettingsWindowModel(UiStatus uiState, IAxusServiceMediator serviceMediator, IInteractorProvider interactorProvider, IDialogService dialogService)
    {
        _uiState = uiState;
        _serviceMediator = serviceMediator;
        _interactorProvider = interactorProvider;
        _dialogService = dialogService;

        this.Status = _uiState.SettingsWindow ??= new SettingsWindowStatus();

        var serviceProvider = Bootstrapper.Instance.GetServiceProvider();

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
        this.ProfileDigitalSignature = new ReactiveProperty<OmniDigitalSignature>().AddTo(_disposable);
        this.NewProfileSignatureCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.NewProfileSignatureCommand.Subscribe(async () => await this.NewProfileSignatureAsync()).AddTo(_disposable);
        this.TrustedSignaturesViewModel = serviceProvider.GetRequiredService<SignaturesViewModel>();
        this.BlockedSignaturesViewModel = serviceProvider.GetRequiredService<SignaturesViewModel>();
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

        await this.TrustedSignaturesViewModel!.DisposeAsync();
        await this.BlockedSignaturesViewModel!.DisposeAsync();
    }

    private async Task NewProfileSignatureAsync()
    {
        var name = await _dialogService.ShowSinglelineTextEditAsync();
        var digitalSignature = OmniDigitalSignature.Create(name, OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256);
        this.ProfileDigitalSignature!.Value = digitalSignature;
    }

    private async Task OpenDownloadDirectoryAsync()
    {
        var path = await _dialogService.ShowOpenDirectoryWindowAsync();
        this.FileDownloadDirectory!.Value = path ?? string.Empty;
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
        var serviceConfig = await _serviceMediator.GetConfigAsync(cancellationToken);

        var ProfileUploader = _interactorProvider.GetProfileUploader();
        var ProfileUploaderConfig = await ProfileUploader.GetConfigAsync(cancellationToken);

        var fileDownloader = _interactorProvider.GetFileDownloader();
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
        this.ProfileDigitalSignature!.Value = ProfileUploaderConfig.DigitalSignature;
        this.TrustedSignaturesViewModel!.Signatures!.AddRange(ProfileUploaderConfig.TrustedSignatures);
        this.BlockedSignaturesViewModel!.Signatures!.AddRange(ProfileUploaderConfig.BlockedSignatures);
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
        var ProfileUploaderConfig = new ProfileUploaderConfig(
            this.ProfileDigitalSignature!.Value,
            this.TrustedSignaturesViewModel!.Signatures!.ToArray(),
            this.BlockedSignaturesViewModel!.Signatures!.ToArray()
        );
        var fileDownloaderConfig = new FileDownloaderConfig(this.FileDownloadDirectory!.Value);

        await _serviceMediator.SetConfigAsync(serviceConfig, cancellationToken);

        var ProfileUploader = _interactorProvider.GetProfileUploader();
        await ProfileUploader.SetConfigAsync(ProfileUploaderConfig, cancellationToken);

        var fileDownloader = _interactorProvider.GetFileDownloader();
        await fileDownloader.SetConfigAsync(fileDownloaderConfig, cancellationToken);
    }
}
