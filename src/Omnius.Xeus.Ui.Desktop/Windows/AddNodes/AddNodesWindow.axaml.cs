using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Xeus.Ui.Desktop.Windows.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Windows.AddNodes
{
    public class AddNodesWindow : StatefulWindowBase
    {
        public AddNodesWindow()
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
            if (this.ViewModel is AddNodesWindowViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
        }

        protected override async ValueTask OnDisposeAsync()
        {
            if (this.ViewModel is AddNodesWindowViewModel viewModel)
            {
                await viewModel.DisposeAsync();
            }
        }

        public AddNodesWindowViewModel? ViewModel
        {
            get => this.DataContext as AddNodesWindowViewModel;
            set => this.DataContext = value;
        }
    }
}
