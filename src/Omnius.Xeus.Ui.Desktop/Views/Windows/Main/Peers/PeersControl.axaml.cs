using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Omnius.Xeus.Ui.Desktop.Views.Windows.Main.Peers
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

        public PeersControlModel? Model
        {
            get => this.DataContext as PeersControlModel;
            set => this.DataContext = value;
        }
    }
}
