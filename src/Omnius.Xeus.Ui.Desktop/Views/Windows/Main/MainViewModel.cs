using System.Reactive.Disposables;
using System.Threading.Tasks;
using Omnius.Core;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.ViewModels
{
    public class MainWindowViewModel : AsyncDisposableBase
    {
        private readonly CompositeDisposable _disposable = new();

        public MainWindowViewModel()
        {
            this.TreeViewWidth = new ReactivePropertySlim<double>().AddTo(_disposable);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _disposable.Dispose();
        }

        public ReactivePropertySlim<double> TreeViewWidth { get; }
    }
}
