using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Controls;
using Omnius.Core;
using Omnius.Xeus.Service.Models;
using Omnius.Xeus.Service.Presenters;
using Omnius.Xeus.Ui.Desktop.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.ViewModels
{
    public sealed class FileSearchControlViewModelOptions
    {
        public IFileFinder? FileFinder { get; set; }
    }

    public class FileSearchControlViewModel : AsyncDisposableBase
    {
        private readonly IFileFinder _fileFinder;

        private readonly ObservableCollection<XeusFileFoundResult> _currentXeusFileFoundResults = new();

        private CompositeDisposable _disposable = new();

        public FileSearchControlViewModel(FileSearchControlViewModelOptions options)
        {
            _fileFinder = options.FileFinder ?? throw new ArgumentNullException(nameof(options.FileFinder));

            this.TreeViewWidth = new ReactivePropertySlim<GridLength>().AddTo(_disposable);
            this.CurrentItems = _currentXeusFileFoundResults.ToReadOnlyReactiveCollection(n => n).AddTo(_disposable);

            this.TreeViewWidth.Value = new GridLength(200);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _disposable.Dispose();
        }

        public ReactivePropertySlim<GridLength> TreeViewWidth { get; }

        public ReadOnlyReactiveCollection<XeusFileFoundResult> CurrentItems { get; }
    }
}
