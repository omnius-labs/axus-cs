using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Omnius.Xeus.Ui.Desktop.Windows.Primitives
{
    public abstract class StatefulWindowBase : Window
    {
        private bool _isInitialized = false;
        private bool _isDisposing = false;
        private bool _isDisposed = false;

        public StatefulWindowBase()
        {
            this.Activated += (_, _) => this.OnActivated();
        }

        private async void OnActivated()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            await this.OnInitializeAsync();
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            if (_isDisposed) return;

            e.Cancel = true;

            if (_isDisposing) return;

            _isDisposing = true;

            await this.OnDisposeAsync();

            _isDisposed = true;

            this.Close();
        }

        protected abstract ValueTask OnInitializeAsync();

        protected abstract ValueTask OnDisposeAsync();
    }
}
