using Avalonia.Markup.Xaml;
using Omnius.Core.Avalonia;

namespace Omnius.Axus.Ui.Desktop.Windows.Settings;

public partial class SignaturesView : StatefulUserControl<SignaturesViewViewModelBase>
{
    public SignaturesView()
    {
        this.InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
