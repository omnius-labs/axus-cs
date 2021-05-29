using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Omnius.Core;
using Omnius.Xeus.Services;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

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

        private readonly IDashboard _dashboard;

        private readonly Task _refreshTask;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly CompositeDisposable _disposable = new();

        public StatusControlViewModel(IDashboard dashboard)
        {
            _dashboard = dashboard;

            this.MyNodeProfile = new ReactiveProperty<string>().AddTo(_disposable);

            _refreshTask = this.RefreshAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();

            await _refreshTask;

            _cancellationTokenSource.Dispose();
        }

        public ReactiveProperty<string> MyNodeProfile { get; }

        private async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                for (; ; )
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

                    var myNodeProfile = await _dashboard.GetMyNodeProfileAsync(cancellationToken);

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        this.MyNodeProfile.Value = XeusMessageConverter.NodeProfileToString(myNodeProfile);
                    });
                }
            }
            catch (OperationCanceledException e)
            {
                _logger.Debug(e);
            }
        }
    }
}
