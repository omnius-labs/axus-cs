using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Omnius.Xeus.Ui.Desktop.Views.Windows.Main.Dashboard
{
    public partial class DashboardControl : UserControl
    {
        public DashboardControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public DashboardControlModel? Model
        {
            get => this.DataContext as DashboardControlModel;
            set => this.DataContext = value;
        }
    }
}
