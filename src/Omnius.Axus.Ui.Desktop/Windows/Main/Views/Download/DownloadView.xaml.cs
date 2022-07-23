using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Omnius.Axus.Ui.Desktop.Windows.Main;

public partial class DownloadView : UserControl
{
    public DownloadView()
    {
        this.InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
