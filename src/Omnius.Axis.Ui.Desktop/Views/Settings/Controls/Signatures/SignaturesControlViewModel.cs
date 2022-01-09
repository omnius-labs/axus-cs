using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Text;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Omnius.Core.Cryptography;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axis.Ui.Desktop.Views.Settings;

public interface ISignaturesControlViewModel
{
    ReactiveCommand RegisterCommand { get; }

    ObservableCollection<OmniSignature> Signatures { get; }

    ObservableCollection<OmniSignature> SelectedSignatures { get; }

    ReactiveCommand ItemDeleteCommand { get; }

    ReactiveCommand ItemCopySeedCommand { get; }
}

public class SignaturesControlViewModel : AsyncDisposableBase, ISignaturesControlViewModel
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly UiStatus _uiState;
    private readonly IIntaractorProvider _intaractorAdapter;
    private readonly IApplicationDispatcher _applicationDispatcher;
    private readonly IDialogService _dialogService;
    private readonly IClipboardService _clipboardService;

    private readonly CompositeDisposable _disposable = new();

    public SignaturesControlViewModel(UiStatus uiState, IIntaractorProvider intaractorAdapter, IApplicationDispatcher applicationDispatcher, IDialogService dialogService, IClipboardService clipboardService)
    {
        _uiState = uiState;
        _intaractorAdapter = intaractorAdapter;
        _applicationDispatcher = applicationDispatcher;
        _dialogService = dialogService;
        _clipboardService = clipboardService;

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
        var text = await _dialogService.ShowMultiLineTextBoxWindowAsync();

        foreach (var item in ParseSignateres(text))
        {
            this.Signatures.Add(item);
        }
    }

    private static IEnumerable<OmniSignature> ParseSignateres(string text)
    {
        var results = new List<OmniSignature>();

        foreach (var line in text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(n => n.Trim()))
        {
            if (OmniSignature.TryParse(line, out var signature)) continue;
            results.Add(signature!);
        }

        return results;
    }

    private async void ItemDelete()
    {
        foreach (var signature in this.SelectedSignatures.ToArray())
        {
            this.Signatures.Remove(signature);
        }
    }

    private async void ItemCopySeed()
    {
        var sb = new StringBuilder();

        foreach (var signature in this.SelectedSignatures.ToArray())
        {
            sb.AppendLine(signature.ToString());
        }

        await _clipboardService.SetTextAsync(sb.ToString());
    }
}
