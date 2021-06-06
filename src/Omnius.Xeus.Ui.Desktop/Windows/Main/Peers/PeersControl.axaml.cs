using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Omnius.Xeus.Ui.Desktop.Windows.Main.Peers
{
    public partial class PeersControl : UserControl
    {
        public PeersControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
