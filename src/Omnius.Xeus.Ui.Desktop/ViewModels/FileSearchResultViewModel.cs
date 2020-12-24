using System.Reactive.Disposables;
using Avalonia.Media.Imaging;
using Omnius.Core;
using Omnius.Xeus.Ui.Desktop.Interactors.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.Interactors.Models
{
    public sealed class FileSearchResultViewModel : DisposableBase
    {
        private readonly CompositeDisposable _disposable = new();

        public FileSearchResultViewModel(FileSearchResultModel model)
        {
            this.Model = model;
            this.Name = this.Model.ObserveProperty(n => n.Name).ToReadOnlyReactivePropertySlim().AddTo(_disposable);
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                _disposable.Dispose();
            }
        }

        public FileSearchResultModel Model { get; }

        public ReadOnlyReactivePropertySlim<string> Name { get; }
    }
}
