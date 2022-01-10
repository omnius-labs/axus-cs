using System.Reactive.Disposables;
using Avalonia.Controls;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axis.Ui.Desktop.Views.Dialogs;

public abstract class MultiLineTextBoxWindowViewModelBase : DisposableBase
{
    public MultiLineTextBoxWindowStatus? Status { get; protected set; }

    public ReactivePropertySlim<string>? Text { get; protected set; }

    public AsyncReactiveCommand? OkCommand { get; protected set; }

    public AsyncReactiveCommand? CancelCommand { get; protected set; }
}

public class MultiLineTextBoxWindowViewModel : MultiLineTextBoxWindowViewModelBase
{
    private readonly UiStatus _uiState;
    private readonly IClipboardService _clipboardService;

    private readonly CompositeDisposable _disposable = new();

    public MultiLineTextBoxWindowViewModel(UiStatus uiState, IClipboardService clipboardService)
    {
        _uiState = uiState;
        _clipboardService = clipboardService;

        this.Status = _uiState.MultiLineTextBoxWindow ?? new MultiLineTextBoxWindowStatus();

        this.Text = new ReactivePropertySlim<string>().AddTo(_disposable);

        this.OkCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.OkCommand.Subscribe((state) => this.OkAsync(state)).AddTo(_disposable);

        this.CancelCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.CancelCommand.Subscribe((state) => this.CancelAsync(state)).AddTo(_disposable);

        this.Initialize();
    }

    public async void Initialize()
    {
        this.Text.Value = await _clipboardService.GetTextAsync();
    }

    protected override void OnDispose(bool disposing)
    {
        if (disposing)
        {
            _disposable.Dispose();
        }
    }

    public string GetResult() => this.Text.Value;

    private async Task OkAsync(object state)
    {
        var window = (Window)state;
        window.Close();
    }

    private async Task CancelAsync(object state)
    {
        this.Text.Value = string.Empty;

        var window = (Window)state;
        window.Close();
    }
}
