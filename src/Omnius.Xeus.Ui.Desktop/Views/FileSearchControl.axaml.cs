using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Omnius.Xeus.Ui.Desktop.ViewModels;

namespace Omnius.Xeus.Ui.Desktop.Views
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

        public FileSearchControlViewModel? ViewModel
        {
            get => this.DataContext as FileSearchControlViewModel;
            set => this.DataContext = value;
        }
    }
}
