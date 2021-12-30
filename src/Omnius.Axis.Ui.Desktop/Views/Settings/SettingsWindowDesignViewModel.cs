using System.Reactive.Disposables;
using Avalonia.Controls;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Reactive.Bindings;

namespace Omnius.Axis.Ui.Desktop.Views.Settings;

public class SettingsWindowDesignViewModel : AsyncDisposableBase, ISettingsWindowViewModel
{
    private readonly CompositeDisposable _disposable = new();

    public SettingsWindowDesignViewModel()
    {
        this.Status = new SettingsWindowStatus();

        this.TrustedSignaturesControlViewModel = new SignaturesControlDesignViewModel();
        this.TrustedSignaturesControlViewModel.Signatures.Add(OmniDigitalSignature.Create("abcd", OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256).GetOmniSignature());

        this.BlockedSignaturesControlViewModel = new SignaturesControlDesignViewModel();
        this.BlockedSignaturesControlViewModel.Signatures.Add(OmniDigitalSignature.Create("efgh", OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256).GetOmniSignature());

        this.DownloadDirectory = new ReactiveProperty<string>().ToAdd(_disposable);

        this.EditDownloadDirectoryCommand = new ReactiveCommand().ToAdd(_disposable);
        this.EditDownloadDirectoryCommand.Subscribe(() => this.OpenDownloadDirectory()).ToAdd(_disposable);

        this.OkCommand = new ReactiveCommand().ToAdd(_disposable);
        this.OkCommand.Subscribe(state => this.Ok(state)).ToAdd(_disposable);

        this.CancelCommand = new ReactiveCommand().ToAdd(_disposable);
        this.CancelCommand.Subscribe(state => this.Cancel(state)).ToAdd(_disposable);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();
    }

    public SettingsWindowStatus Status { get; }

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
    }

    private async void Cancel(object state)
    {
        var window = (Window)state;
        window.Close();
    }
}
