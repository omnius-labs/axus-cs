using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Omnius.Axis.Ui.Desktop.Views.Main;

public partial class DownloadControl : UserControl
{
    public DownloadControl()
    {
        this.InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
