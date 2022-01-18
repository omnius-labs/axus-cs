using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Omnius.Axis.Ui.Desktop.Views.Main;

public partial class SearchControl : UserControl
{
    public SearchControl()
    {
        this.InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
