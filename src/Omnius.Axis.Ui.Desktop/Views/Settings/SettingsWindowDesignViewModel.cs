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
        this.TrustedSignaturesControlViewModel.SetSignatures(new[] { OmniDigitalSignature.Create("abcd", OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256).GetOmniSignature() });

        this.BlockedSignaturesControlViewModel = new SignaturesControlDesignViewModel();
        this.BlockedSignaturesControlViewModel.SetSignatures(new[] { OmniDigitalSignature.Create("efgh", OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256).GetOmniSignature() });

        this.DownloadDirectory = new ReactiveProperty<string>().AddTo(_disposable);

        this.EditDownloadDirectoryCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.EditDownloadDirectoryCommand.Subscribe(async () => await this.EditDownloadDirectoryAsync()).AddTo(_disposable);

        this.OkCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.OkCommand.Subscribe(async (state) => await this.OkAsync(state)).AddTo(_disposable);

        this.CancelCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.CancelCommand.Subscribe(async (state) => await this.CancelAsync(state)).AddTo(_disposable);
    }

    protected override async ValueTask OnDisposeAsync()
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
