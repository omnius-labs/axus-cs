using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;

namespace Omnius.Xeus.Ui.Desktop.Windows.Main.Upload
{
    public interface IUploadControlViewModel
    {
    }

    public class DesignUploadControlViewModel : DisposableBase, IUploadControlViewModel
    {
        protected override void OnDispose(bool disposing)
        {
        }
    }

    public class UploadControlViewModel : AsyncDisposableBase, IUploadControlViewModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public UploadControlViewModel()
        {
        }

        protected override async ValueTask OnDisposeAsync()
        {
        }
    }
}
