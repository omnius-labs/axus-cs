using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Omnius.Xeus.Service.Models;
using Omnius.Xeus.Ui.Desktop.Windows.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Windows.Dialogs
{
    public interface IAddNodesWindowFactory
    {
        IAddNodesWindow Create();
    }

    public interface IAddNodesWindow
    {
        ValueTask<IEnumerable<NodeLocation>> ShowDialogAsync(Window owner);
    }

    public class AddNodesWindow : StatefulWindowBase, IAddNodesWindow
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

        public IEnumerable<NodeLocation> ShowDialogAsync(Window owner)
        {
            throw new System.NotImplementedException();
        }

        ValueTask<IEnumerable<NodeLocation>> IAddNodesWindow.ShowDialogAsync(Window owner)
        {
            throw new System.NotImplementedException();
        }

        public AddNodesWindowViewModel? ViewModel
        {
            get => this.DataContext as AddNodesWindowViewModel;
            set => this.DataContext = value;
        }
    }
}
