using System.Reactive.Disposables;
using Omnius.Core.Cryptography;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axus.Ui.Desktop.Windows.Settings;

public class SignaturesViewDesignModel : SignaturesViewModelBase
{
    private readonly CompositeDisposable _disposable = new();

    public SignaturesViewDesignModel()
    {
        this.Title = new ReactivePropertySlim<string>().AddTo(_disposable);

        this.AddCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.AddCommand.Subscribe(async () => await this.RegisterAsync()).AddTo(_disposable);

        var sampleSignature1 = OmniDigitalSignature.Create("sample1", OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256).GetOmniSignature();
        var sampleSignature2 = OmniDigitalSignature.Create("sample2", OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256).GetOmniSignature();

        this.Signatures = new(new[] { sampleSignature1, sampleSignature2 });
        this.SelectedSignatures = new();

        this.ItemDeleteCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.ItemDeleteCommand.Subscribe(async () => await this.ItemDeleteAsync()).AddTo(_disposable);

        this.ItemCopySignatureCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.ItemCopySignatureCommand.Subscribe(async () => await this.ItemCopySeedAsync()).AddTo(_disposable);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();
    }

    public override string GetTitle()
    {
        return this.Title?.Value ?? string.Empty;
    }

    public override void SetTitle(string title)
    {
        this.Title!.Value = title;
    }

    public override IEnumerable<OmniSignature> GetSignatures()
    {
        return Array.Empty<OmniSignature>();
    }

    public override void SetSignatures(IEnumerable<OmniSignature> signatures)
    {
    }

    private async Task RegisterAsync()
    {
    }

    private async Task ItemDeleteAsync()
    {
    }

    private async Task ItemCopySeedAsync()
    {
    }
}
