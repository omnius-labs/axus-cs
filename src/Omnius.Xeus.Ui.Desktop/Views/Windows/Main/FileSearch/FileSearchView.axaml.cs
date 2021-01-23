using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Omnius.Xeus.Api;
using Omnius.Xeus.Ui.Desktop.ViewModels;

namespace Omnius.Xeus.Ui.Desktop.Views
{
    public class FileSearchControl : UserControl, IAsyncDisposable
    {
        public FileSearchControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private FileSearchControlViewModel? ViewModel
        {
            get => this.DataContext as FileSearchControlViewModel;
            set => this.DataContext = value;
        }

        public async ValueTask DisposeAsync()
        {
            if (this.ViewModel is FileSearchControlViewModel viewModel)
            {
                await viewModel.DisposeAsync();
            }
        }
    }
}
