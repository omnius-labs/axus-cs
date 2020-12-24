using System.Reactive.Disposables;
using System.Threading.Tasks;
using Omnius.Core;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.ViewModels
{
    public class MainWindowViewModel : AsyncDisposableBase
    {
        private CompositeDisposable _disposable = new();

        public MainWindowViewModel()
        {
            this.TreeViewWidth = new ReactivePropertySlim<double>().AddTo(_disposable);
            this.FileSearchControlViewModel = new FileSearchControlViewModel();
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _disposable.Dispose();
            await this.FileSearchControlViewModel.DisposeAsync();
        }

        public FileSearchControlViewModel FileSearchControlViewModel { get; }

        public ReactivePropertySlim<double> TreeViewWidth { get; }
    }
}
