using Avalonia.Controls;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Reactive.Bindings;

namespace Omnius.Axis.Ui.Desktop.Views.Dialogs;

public abstract class MultiLineTextInputWindowViewModelBase : AsyncDisposableBase
{
    public MultiLineTextInputWindowStatus? Status { get; protected set; }

    public ReactivePropertySlim<string>? Text { get; protected set; }

    public AsyncReactiveCommand? OkCommand { get; protected set; }

    public AsyncReactiveCommand? CancelCommand { get; protected set; }

    public abstract string GetResult();
}

public class MultiLineTextInputWindowViewModel : MultiLineTextInputWindowViewModelBase
{
    private readonly UiStatus _uiState;
    private readonly IClipboardService _clipboardService;

    private readonly CompositeDisposable _disposable = new();

    public MultiLineTextInputWindowViewModel(UiStatus uiState, IClipboardService clipboardService)
    {
        _uiState = uiState;
        _clipboardService = clipboardService;

        this.Status = _uiState.MultiLineTextInputWindow ??= new MultiLineTextInputWindowStatus();

        this.Text = new ReactivePropertySlim<string>().AddTo(_disposable);

        this.OkCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.OkCommand.Subscribe(async (state) => await this.OkAsync(state)).AddTo(_disposable);

        this.CancelCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.CancelCommand.Subscribe(async (state) => await this.CancelAsync(state)).AddTo(_disposable);

        this.Initialize();
    }

    public async void Initialize()
    {
        this.Text!.Value = await _clipboardService.GetTextAsync();
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();
    }

    public override string GetResult() => this.Text!.Value;

    private async Task OkAsync(object state)
    {
        var window = (Window)state;
        window.Close();
    }

    private async Task CancelAsync(object state)
    {
        this.Text!.Value = string.Empty;

        var window = (Window)state;
        window.Close();
    }
}
