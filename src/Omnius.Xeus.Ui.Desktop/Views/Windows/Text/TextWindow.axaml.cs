using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using Omnius.Xeus.Ui.Desktop.Windows.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Windows
{
    public class TextWindow : StatefulWindowBase
    {
        public TextWindow()
            : base()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override async ValueTask OnInitializeAsync()
        {
            if (this.ViewModel is TextWindowViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
        }

        protected override async ValueTask OnDisposeAsync()
        {
            if (this.ViewModel is TextWindowViewModel viewModel)
            {
                await viewModel.DisposeAsync();
            }
        }

        public TextWindowViewModel? ViewModel
        {
            get => this.DataContext as TextWindowViewModel;
            set => this.DataContext = value;
        }
    }
}
