using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Omnius.Xeus.Ui.Desktop.Views.Windows.Main.Upload
{
    public partial class UploadControl : UserControl
    {
        public UploadControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public UploadControlModel? Model
        {
            get => this.DataContext as UploadControlModel;
            set => this.DataContext = value;
        }
    }
}
