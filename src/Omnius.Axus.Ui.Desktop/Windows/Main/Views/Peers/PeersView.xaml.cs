using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Omnius.Axus.Ui.Desktop.Windows.Main;

public partial class PeersView : UserControl
{
    public PeersView()
    {
        this.InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
