using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;

namespace Omnius.Xeus.Ui.Desktop.Windows.Main.Status
{
    public interface IStatusControlViewModel
    {
    }

    public class DesignStatusControlViewModel : DisposableBase, IStatusControlViewModel
    {
        protected override void OnDispose(bool disposing)
        {
        }
    }

    public class StatusControlViewModel : AsyncDisposableBase, IStatusControlViewModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public StatusControlViewModel()
        {
        }

        protected override async ValueTask OnDisposeAsync()
        {
        }
    }
}
