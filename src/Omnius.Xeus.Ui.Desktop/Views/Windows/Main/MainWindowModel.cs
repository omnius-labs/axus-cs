using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Ui.Desktop.Resources;

namespace Omnius.Xeus.Ui.Desktop.Views.Windows.Main
{
    public class MainWindowModel : AsyncDisposableBase
    {
        private readonly AppState _state;

        public MainWindowModel(AppState state)
        {
            _state = state;
        }

        protected override async ValueTask OnDisposeAsync()
        {
        }
    }
}
