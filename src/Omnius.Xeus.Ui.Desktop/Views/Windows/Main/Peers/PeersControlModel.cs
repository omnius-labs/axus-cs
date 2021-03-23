using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Omnius.Core;
using Omnius.Core.Extensions;
using Omnius.Core.Network;
using Omnius.Lxna.Ui.Desktop.Presenters;
using Omnius.Lxna.Ui.Desktop.Presenters.Primitives;
using Omnius.Xeus.Ui.Desktop.Resources;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using EnginesModels = Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Ui.Desktop.Views.Windows.Main.Peers
{
    public class PeersControlModel : AsyncDisposableBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly AppState _state;
        private readonly Task _refreshTask;

        private readonly ObservableDictionary<OmniAddress, ConnectionReportViewModel> _connectionReportMap = new();

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly CompositeDisposable _disposable = new();

        public PeersControlModel(AppState status)
        {
            _state = status;
            _refreshTask = this.RefreshAsync(_cancellationTokenSource.Token);

            var uiState = _state.GetUiState();

            this.ConnectionReports = _connectionReportMap.Values.ToReadOnlyReactiveCollection().AddTo(_disposable);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await _refreshTask;
            _cancellationTokenSource.Dispose();

            _disposable.Dispose();
        }

        public ReadOnlyReactiveCollection<ConnectionReportViewModel> ConnectionReports { get; }

        private async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // FIXME
                {
                    var dashboard = _state.GetDashboard();
                    await dashboard.AddCloudNodeProfileAsync(new[] { new EnginesModels.NodeProfile(new[] { OmniAddress.Parse("tcp(ip4(127.0.0.1),41000)") }, new[] { "ckad_mediator" }) }, cancellationToken);
                }

                for (; ; )
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

                    var dashboard = _state.GetDashboard();
                    var connectionReports = await dashboard.GetConnectionReportsAsync(cancellationToken);

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
                                _connectionReportMap.Add(report.Address, new ConnectionReportViewModel(report));
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
