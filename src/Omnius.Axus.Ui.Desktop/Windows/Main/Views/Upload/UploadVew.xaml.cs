using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Omnius.Axus.Ui.Desktop.Windows.Main;

public partial class UploadView : UserControl
{
    public UploadView()
    {
        this.InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
