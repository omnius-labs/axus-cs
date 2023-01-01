using System.Collections.ObjectModel;
using System.Text;
using Omnius.Axus.Interactors;
using Omnius.Axus.Ui.Desktop.Internal;
using Omnius.Axus.Ui.Desktop.Configuration;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Omnius.Core.Cryptography;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axus.Ui.Desktop.Windows.Settings;

public abstract class SignaturesViewModelBase : AsyncDisposableBase
{
    public AsyncReactiveCommand? AddCommand { get; protected set; }
    public ObservableCollection<OmniSignature>? Signatures { get; protected set; }
    public ObservableCollection<OmniSignature>? SelectedSignatures { get; protected set; }
    public AsyncReactiveCommand? ItemDeleteCommand { get; protected set; }
    public AsyncReactiveCommand? ItemCopySignatureCommand { get; protected set; }

    public abstract IEnumerable<OmniSignature> GetSignatures();
    public abstract void SetSignatures(IEnumerable<OmniSignature> signatures);
}

public class SignaturesViewModel : SignaturesViewModelBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly UiStatus _uiState;
    private readonly IInteractorProvider _interactorProvider;
    private readonly IApplicationDispatcher _applicationDispatcher;
    private readonly IDialogService _dialogService;
    private readonly IClipboardService _clipboardService;

    private readonly CompositeDisposable _disposable = new();

    public SignaturesViewModel(UiStatus uiState, IInteractorProvider interactorProvider, IApplicationDispatcher applicationDispatcher, IDialogService dialogService, IClipboardService clipboardService)
    {
        _uiState = uiState;
        _interactorProvider = interactorProvider;
        _applicationDispatcher = applicationDispatcher;
        _dialogService = dialogService;
        _clipboardService = clipboardService;

        this.AddCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.AddCommand.Subscribe(async () => await this.RegisterAsync()).AddTo(_disposable);
        this.Signatures = new();
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

    public override IEnumerable<OmniSignature> GetSignatures()
    {
        return this.Signatures?.ToArray() ?? Array.Empty<OmniSignature>();
    }

    public override void SetSignatures(IEnumerable<OmniSignature> signatures)
    {
        this.Signatures?.Clear();
        this.Signatures?.AddRange(signatures);
    }

    private async Task RegisterAsync()
    {
        var text = await _dialogService.ShowMultilineTextEditAsync();

        foreach (var item in ParseSignatures(text))
        {
            this.Signatures!.Add(item);
        }
    }

    private static IEnumerable<OmniSignature> ParseSignatures(string text)
    {
        var results = new List<OmniSignature>();

        foreach (var line in text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(n => n.Trim()))
        {
            if (!OmniSignature.TryParse(line, out var signature)) continue;
            results.Add(signature!);
        }

        return results;
    }

    private async Task ItemDeleteAsync()
    {
        foreach (var signature in this.SelectedSignatures!.ToArray())
        {
            this.Signatures!.Remove(signature);
        }
    }

    private async Task ItemCopySeedAsync()
    {
        var sb = new StringBuilder();

        foreach (var signature in this.SelectedSignatures!.ToArray())
        {
            sb.AppendLine(signature.ToString());
        }

        await _clipboardService.SetTextAsync(sb.ToString());
    }
}
