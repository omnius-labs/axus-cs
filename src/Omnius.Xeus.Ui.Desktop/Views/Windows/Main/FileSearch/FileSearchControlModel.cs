using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Controls;
using Omnius.Core;
using Omnius.Xeus.Service.Models;
using Omnius.Xeus.Ui.Desktop.Resources;
using Omnius.Xeus.Ui.Desktop.Views.Windows.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.Views.Windows.Main.FileSearch
{
    public class FileSearchControlModel : AsyncDisposableBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly AppState _state;

        private readonly ObservableCollection<XeusFileFoundResult> _currentXeusFileFoundResults = new();

        private CompositeDisposable _disposable = new();

        public FileSearchControlModel(AppState status)
        {
            _state = status;

            var uiSettings = _state.GetUiSettings();

            this.TreeViewWidth = uiSettings.ToReactivePropertySlimAsSynchronized(n => n.FileSearchControl_TreeViewWidth, convert: ConvertHelper.DoubleToGridLength, convertBack: ConvertHelper.GridLengthToDouble).AddTo(_disposable);
            this.CurrentItems = _currentXeusFileFoundResults.ToReadOnlyReactiveCollection(n => n).AddTo(_disposable);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _disposable.Dispose();
        }

        public ReactivePropertySlim<GridLength> TreeViewWidth { get; }

        public ReadOnlyReactiveCollection<XeusFileFoundResult> CurrentItems { get; }
    }
}
