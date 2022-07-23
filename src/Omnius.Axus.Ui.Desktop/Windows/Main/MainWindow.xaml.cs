using Avalonia;
using Avalonia.Markup.Xaml;
using Omnius.Core.Avalonia;

namespace Omnius.Axus.Ui.Desktop.Windows.Main;

public partial class MainWindow : StatefulWindowBase<MainWindowModelBase>
{
    public MainWindow()
        : base()
    {
        this.InitializeComponent();
    }

    public MainWindow(string configDirectoryPath)
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

    private async void OnClosed()
    {
        if (this.ViewModel is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
    }
}
