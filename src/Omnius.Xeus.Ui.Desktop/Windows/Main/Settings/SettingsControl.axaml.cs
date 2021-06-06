using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Omnius.Xeus.Ui.Desktop.Windows.Main.Settings
{
    public partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
