using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Controls;
using Omnius.Core;
using Omnius.Xeus.Ui.Desktop.Service.Models;
using Omnius.Xeus.Ui.Desktop.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.ViewModels
{
    public sealed class FileSearchControlViewModelOptions
    {
    }

    public class FileSearchControlViewModel : AsyncDisposableBase
    {
        private readonly ObservableCollection<FileSearchResultModel> _currentFileSearchResultModels = new();
        private readonly FileSearchControlViewModelOptions _options;
        private CompositeDisposable _disposable = new();

        public FileSearchControlViewModel(FileSearchControlViewModelOptions options)
        {
            this.TreeViewWidth = new ReactivePropertySlim<GridLength>().AddTo(_disposable);
            this.CurrentItems = _currentFileSearchResultModels.ToReadOnlyReactiveCollection(n => new FileSearchResultViewModel(n)).AddTo(_disposable);

            this.TreeViewWidth.Value = new GridLength(200);
            _currentFileSearchResultModels.Add(new FileSearchResultModel() { Name = "test" });
            _options = options;
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _disposable.Dispose();
        }

        public ReactivePropertySlim<GridLength> TreeViewWidth { get; }

        public ReadOnlyReactiveCollection<FileSearchResultViewModel> CurrentItems { get; }
    }
}
