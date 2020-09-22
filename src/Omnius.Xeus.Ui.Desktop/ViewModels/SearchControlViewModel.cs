using System.Reactive.Disposables;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.ViewModels
{
    public class SearchControlViewModel : ViewModelBase
    {
        private CompositeDisposable _disposables = new();

        public SearchControlViewModel()
        {
            this.TreeViewWidth = new ReactivePropertySlim<double>().AddTo(_disposables);
        }

        protected override void OnDispose(bool disposing)
        {
            _disposables.Dispose();
        }

        public ReactivePropertySlim<double> TreeViewWidth { get; }
    }
}
