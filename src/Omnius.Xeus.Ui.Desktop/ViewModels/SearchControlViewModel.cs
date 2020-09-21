using System;
using System.Collections.Generic;
using System.Text;
using Reactive.Bindings;
using System.Reactive.Disposables;
using Reactive.Bindings.Extensions;
using Omnius.Core;

namespace Omnius.Xeus.Ui.Desktop.ViewModels
{
    public class SearchControlViewModel : DisposableBase
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
