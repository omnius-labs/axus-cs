using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Omnius.Xeus.Ui.Desktop.Views.Windows.Main.FileSearch
{
    public class FileSearchControl : UserControl
    {
        public FileSearchControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public FileSearchControlModel? Model
        {
            get => this.DataContext as FileSearchControlModel;
            set => this.DataContext = value;
        }
    }
}
