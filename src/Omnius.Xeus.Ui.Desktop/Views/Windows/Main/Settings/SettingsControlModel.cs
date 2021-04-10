using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Ui.Desktop.Resources;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.Views.Windows.Main.Settings
{
    public interface ISettingsControlModel
    {
        ReadOnlyReactiveCollection<object> Elements { get; }
    }

    public class DesignSettingsControlModel : DisposableBase, ISettingsControlModel
    {
        private readonly CompositeDisposable _disposable = new();

        public DesignSettingsControlModel()
        {
            var elements = new ObservableCollection<object>();
            this.Elements = elements.ToReadOnlyReactiveCollection().AddTo(_disposable);
        }

        protected override void OnDispose(bool disposing)
        {
            _disposable.Dispose();
        }

        public ReadOnlyReactiveCollection<object> Elements { get; }
    }

    public class SettingsControlModel : AsyncDisposableBase, ISettingsControlModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly AppState _state;
        private readonly Task _refreshTask;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly CompositeDisposable _disposable = new();

        public SettingsControlModel(AppState status)
        {
            _state = status;
            _refreshTask = this.RefreshAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await _refreshTask;
            _cancellationTokenSource.Dispose();

            _disposable.Dispose();
        }

        public ReadOnlyReactiveCollection<object> Elements { get; }

        private async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            try
            {
            }
            catch (OperationCanceledException e)
            {
                _logger.Debug(e);
            }
        }
    }
}
