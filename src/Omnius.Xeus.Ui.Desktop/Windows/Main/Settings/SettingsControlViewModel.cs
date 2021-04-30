using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;

namespace Omnius.Xeus.Ui.Desktop.Windows.Main.Settings
{
    public interface ISettingsControlViewModel
    {
    }

    public class DesignSettingsControlViewModel : DisposableBase, ISettingsControlViewModel
    {
        protected override void OnDispose(bool disposing)
        {
        }
    }

    public class SettingsControlViewModel : AsyncDisposableBase, ISettingsControlViewModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public SettingsControlViewModel()
        {
        }

        protected override async ValueTask OnDisposeAsync()
        {
        }
    }
}
