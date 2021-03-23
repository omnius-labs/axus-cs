using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Omnius.Xeus.Ui.Desktop.Views.Windows.Main.Status
{
    public partial class StatusControl : UserControl
    {
        public StatusControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public StatusControlModel? Model
        {
            get => this.DataContext as StatusControlModel;
            set => this.DataContext = value;
        }
    }
}
