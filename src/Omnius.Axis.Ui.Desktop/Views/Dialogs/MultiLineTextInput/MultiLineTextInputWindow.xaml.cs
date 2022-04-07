using Avalonia;
using Avalonia.Markup.Xaml;
using Omnius.Core.Avalonia;

namespace Omnius.Axis.Ui.Desktop.Views.Dialogs;

public partial class MultiLineTextInputWindow : StatefulWindowBase<MultiLineTextInputWindowViewModelBase>
{
    private string? _result = null;

    public MultiLineTextInputWindow()
        : base()
    {
        this.InitializeComponent();
    }

    public MultiLineTextInputWindow(string configDirectoryPath)
        : base(configDirectoryPath)
    {
        this.InitializeComponent();

#if DEBUG
        this.AttachDevTools();
#endif

        this.Closed += new EventHandler((_, _) => this.OnClosed());
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public string? GetResult() => _result;

    private async void OnClosed()
    {
        _result = this.ViewModel?.GetResult() ?? string.Empty;

        if (this.ViewModel is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
    }
}
