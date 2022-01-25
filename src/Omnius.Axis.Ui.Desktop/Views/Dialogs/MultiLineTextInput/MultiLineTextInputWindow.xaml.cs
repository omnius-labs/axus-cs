using System.ComponentModel;
using Avalonia;
using Avalonia.Markup.Xaml;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Core.Avalonia;

namespace Omnius.Axis.Ui.Desktop.Views.Dialogs;

public partial class MultiLineTextInputWindow : StatefulWindowBase<MultiLineTextInputWindowViewModelBase>
{
    private string? _result = null;

    public MultiLineTextInputWindow()
        : base()
    {
        this.InitializeComponent();
        this.GetObservable(ViewModelProperty).Subscribe(this.OnViewModelChanged);
        this.Closing += new EventHandler<CancelEventArgs>((_, _) => this.OnClosing());
        this.Closed += new EventHandler((_, _) => this.OnClosed());
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public string? GetResult() => _result;

    private void OnViewModelChanged(MultiLineTextInputWindowViewModelBase? viewModel)
    {
        if (viewModel?.Status is MultiLineTextInputWindowStatus status)
        {
            this.SetWindowStatus(status.Window);
        }
    }

    private void OnClosing()
    {
        if (this.ViewModel?.Status is MultiLineTextInputWindowStatus status)
        {
            status.Window = this.GetWindowStatus();
        }
    }

    private async void OnClosed()
    {
        _result = this.ViewModel?.GetResult() ?? string.Empty;

        if (this.ViewModel is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
    }
}
