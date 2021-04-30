using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;

namespace Omnius.Xeus.Ui.Desktop.Windows.Main.Download
{
    public interface IDownloadControlViewModel
    {
    }

    public class DesignDownloadControlViewModel : DisposableBase, IDownloadControlViewModel
    {
        protected override void OnDispose(bool disposing)
        {
        }
    }

    public class DownloadControlViewModel : AsyncDisposableBase, IDownloadControlViewModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public DownloadControlViewModel()
        {
        }

        protected override async ValueTask OnDisposeAsync()
        {
        }
    }
}
