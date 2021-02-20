using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Omnius.Core;
using Omnius.Xeus.Ui.Desktop.Resources;
using Omnius.Xeus.Ui.Desktop.Views.Windows.Main.FileSearch;
using Omnius.Xeus.Ui.Desktop.Views.Windows.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Views.Windows.Main
{
    public class MainWindow : StatefulWindowBase
    {
        private AppState? _state;

        public MainWindow()
            : base()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override async ValueTask OnInitialize()
        {
            var args = App.Current.Lifetime!.Args ?? Array.Empty<string>();

            var stateDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), args[0]);
            var bytesPool = BytesPool.Shared;

            _state = await AppState.Factory.CreateAsync(stateDirectoryPath, bytesPool);

            this.Model = new MainWindowModel(_state);
            this.FileSearchControl.Model = new FileSearchControlModel(_state);
        }

        protected override async ValueTask OnDispose()
        {
            if (this.FileSearchControl.Model is FileSearchControlModel fileViewControlModel)
            {
                await fileViewControlModel.DisposeAsync();
            }

            if (this.Model is MainWindowModel mainWindowModel)
            {
                await mainWindowModel.DisposeAsync();
            }

            if (_state is not null)
            {
                await _state.DisposeAsync();
            }
        }

        public MainWindowModel? Model
        {
            get => this.DataContext as MainWindowModel;
            set => this.DataContext = value;
        }

        public FileSearchControl FileSearchControl => this.FindControl<FileSearchControl>(nameof(this.FileSearchControl));
    }
}
