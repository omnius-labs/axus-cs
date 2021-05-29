using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Services;
using Reactive.Bindings;

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

            _refreshTask = this.RefreshAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {
        }

        public ReactiveProperty<string> MyNodeProfile { get; }

        private async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // FIXME
                {
                    await _dashboard.AddCloudNodeProfileAsync(new[] { new EnginesModels.NodeProfile(new[] { OmniAddress.Parse("tcp(ip4(127.0.0.1),41000)") }, new[] { "ckad_mediator" }) }, cancellationToken);
                }

                for (; ; )
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

                    var connectionReports = await _dashboard.GetConnectionReportsAsync(cancellationToken);

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        foreach (var address in _connectionReportMap.Keys.ToArray())
                        {
                            if (connectionReports.Any(n => n.Address == address)) continue;
                            _connectionReportMap.Remove(address);
                        }

                        foreach (var report in connectionReports)
                        {
                            if (!_connectionReportMap.TryGetValue(report.Address, out var viewModel))
                            {
                                _connectionReportMap.Add(report.Address, new ConnectionReportElement(report));
                            }
                            else
                            {
                                viewModel.Model = report;
                            }
                        }
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
