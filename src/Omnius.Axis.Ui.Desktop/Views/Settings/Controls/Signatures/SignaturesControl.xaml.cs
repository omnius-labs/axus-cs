using Avalonia.Markup.Xaml;
using Omnius.Core.Avalonia;

namespace Omnius.Axis.Ui.Desktop.Views.Settings;

public partial class SignaturesControl : StatefulUserControl<SignaturesControlViewModelBase>
{
    public SignaturesControl()
    {
        this.InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
