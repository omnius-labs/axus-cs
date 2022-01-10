using System.Reactive.Disposables;
using Avalonia.Controls;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Reactive.Bindings;

namespace Omnius.Axis.Ui.Desktop.Views.Settings;

public class SettingsWindowDesignViewModel : SettingsWindowViewModelBase
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

        this.EditDownloadDirectoryCommand = new AsyncReactiveCommand().ToAdd(_disposable);
        this.EditDownloadDirectoryCommand.Subscribe(() => this.EditDownloadDirectoryAsync()).ToAdd(_disposable);

        this.OkCommand = new AsyncReactiveCommand().ToAdd(_disposable);
        this.OkCommand.Subscribe(state => this.OkAsync(state)).ToAdd(_disposable);

        this.CancelCommand = new AsyncReactiveCommand().ToAdd(_disposable);
        this.CancelCommand.Subscribe(state => this.CancelAsync(state)).ToAdd(_disposable);
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
    }

    private async Task CancelAsync(object state)
    {
        var window = (Window)state;
        window.Close();
    }
}
