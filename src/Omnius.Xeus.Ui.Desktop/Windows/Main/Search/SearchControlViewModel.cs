using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;

namespace Omnius.Xeus.Ui.Desktop.Windows.Main.Search
{
    public class SearchControlViewModel : AsyncDisposableBase
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
