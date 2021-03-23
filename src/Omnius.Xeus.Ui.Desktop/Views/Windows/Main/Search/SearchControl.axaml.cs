using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Omnius.Xeus.Ui.Desktop.Views.Windows.Main.Search
{
    public partial class SearchControl : UserControl
    {
        public SearchControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public SearchControlModel? Model
        {
            get => this.DataContext as SearchControlModel;
            set => this.DataContext = value;
        }
    }
}
