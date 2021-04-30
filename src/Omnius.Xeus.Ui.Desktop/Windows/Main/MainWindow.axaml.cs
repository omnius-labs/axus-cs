using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Xeus.Ui.Desktop.Windows.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Windows.Main
{
    public partial class MainWindow : StatefulWindowBase
    {
        public MainWindow()
            : base()
        {
            this.InitializeComponent();

            this.ViewModel = Bootstrapper.ServiceProvider!.GetRequiredService<IMainWindowViewModel>();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override async ValueTask OnInitializeAsync()
        {
        }

        protected override async ValueTask OnDisposeAsync()
        {
            if (this.ViewModel is MainWindowViewModel mainWindowViewModel)
            {
                await mainWindowViewModel.DisposeAsync();
            }
        }

        public IMainWindowViewModel? ViewModel
        {
            get => this.DataContext as IMainWindowViewModel;
            set => this.DataContext = value;
        }
    }
}
