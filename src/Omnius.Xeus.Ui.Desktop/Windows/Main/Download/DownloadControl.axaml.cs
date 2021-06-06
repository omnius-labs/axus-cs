using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Omnius.Xeus.Ui.Desktop.Windows.Main.Download
{
    public partial class DownloadControl : UserControl
    {
        public DownloadControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
