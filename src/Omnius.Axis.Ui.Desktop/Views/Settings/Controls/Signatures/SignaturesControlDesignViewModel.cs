using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axis.Ui.Desktop.Views.Settings;

public class SignaturesControlDesignViewModel : AsyncDisposableBase, ISignaturesControlViewModel
{
    private readonly CompositeDisposable _disposable = new();

    public SignaturesControlDesignViewModel()
    {
        this.RegisterCommand = new ReactiveCommand().AddTo(_disposable);
        this.RegisterCommand.Subscribe(() => this.Register()).AddTo(_disposable);

        this.Signatures = new();
        this.SelectedSignatures = new();

        this.ItemDeleteCommand = new ReactiveCommand().AddTo(_disposable);
        this.ItemDeleteCommand.Subscribe(() => this.ItemDelete()).AddTo(_disposable);

        this.ItemCopySeedCommand = new ReactiveCommand().AddTo(_disposable);
        this.ItemCopySeedCommand.Subscribe(() => this.ItemCopySeed()).AddTo(_disposable);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();
    }

    public ReactiveCommand RegisterCommand { get; }

    public ObservableCollection<OmniSignature> Signatures { get; }

    public ObservableCollection<OmniSignature> SelectedSignatures { get; }

    public ReactiveCommand ItemDeleteCommand { get; }

    public ReactiveCommand ItemCopySeedCommand { get; }

    private async void Register()
    {
    }

    private async void ItemDelete()
    {
    }

    private async void ItemCopySeed()
    {
    }
}
