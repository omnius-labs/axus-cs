using Avalonia;
using Avalonia.Markup.Xaml;
using Omnius.Core.Avalonia;

namespace Omnius.Axus.Ui.Desktop.Windows.Dialogs.MultilineTextEdit;

public partial class MultilineTextEditWindow : StatefulWindowBase<MultilineTextEditWindowModelBase>
{
    private string? _result = null;

    public MultilineTextEditWindow()
        : base()
    {
        this.InitializeComponent();
    }

    public MultilineTextEditWindow(string configDirectoryPath)
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
