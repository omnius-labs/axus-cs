using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;

namespace Omnius.Xeus.Ui.Desktop.Windows.Main.Search
{
    public interface ISearchControlViewModel
    {
    }

    public class DesignSearchControlViewModel : DisposableBase, ISearchControlViewModel
    {
        protected override void OnDispose(bool disposing)
        {
        }
    }

    public class SearchControlViewModel : AsyncDisposableBase, ISearchControlViewModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public SearchControlViewModel()
        {
        }

        protected override async ValueTask OnDisposeAsync()
        {
        }
    }
}
