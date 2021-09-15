using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Omnius.Xeus.Service.Models;
using Omnius.Xeus.Ui.Desktop.Windows.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Windows
{
    public interface INodesWindowFactory
    {
        INodesWindow Create();
    }

    public interface INodesWindow
    {
        ValueTask<IEnumerable<NodeLocation>> ShowDialogAsync(Window owner);
    }

    public class NodesWindow : StatefulWindowBase, INodesWindow
    {
        public NodesWindow()
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
            if (this.ViewModel is NodesWindowViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
        }

        protected override async ValueTask OnDisposeAsync()
        {
            if (this.ViewModel is NodesWindowViewModel viewModel)
            {
                await viewModel.DisposeAsync();
            }
        }

        public IEnumerable<NodeLocation> ShowDialogAsync(Window owner)
        {
            throw new System.NotImplementedException();
        }

        ValueTask<IEnumerable<NodeLocation>> INodesWindow.ShowDialogAsync(Window owner)
        {
            throw new System.NotImplementedException();
        }

        public NodesWindowViewModel? ViewModel
        {
            get => this.DataContext as NodesWindowViewModel;
            set => this.DataContext = value;
        }
    }
}
