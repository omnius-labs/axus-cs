using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Xeus.Interactors;
using Omnius.Xeus.Ui.Desktop.Configuration;
using Omnius.Xeus.Ui.Desktop.Models.Peers;
using Omnius.Xeus.Ui.Desktop.Models.Primitives;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using EnginesModels = Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Ui.Desktop.Windows.Main.Peers
{
    public interface IPeersControlViewModel
    {
        ReadOnlyReactiveCollection<ConnectionReportElement> ConnectionReports { get; }
    }

    public class PeersControlViewModel : AsyncDisposableBase, IPeersControlViewModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly UiState _uiState;
        private readonly IDashboard _dashboard;

        private readonly Task _refreshTask;

        private readonly ObservableDictionary<OmniAddress, ConnectionReportElement> _connectionReportMap = new();

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly CompositeDisposable _disposable = new();

        public PeersControlViewModel(UiState uiStatus, IDashboard dashboard)
        {
            _uiState = uiStatus;
            _dashboard = dashboard;

            _refreshTask = this.RefreshAsync(_cancellationTokenSource.Token);

            this.ConnectionReports = _connectionReportMap.Values.ToReadOnlyReactiveCollection().AddTo(_disposable);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await _refreshTask;
            _cancellationTokenSource.Dispose();

            _disposable.Dispose();
        }

        public ReadOnlyReactiveCollection<ConnectionReportElement> ConnectionReports { get; }

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
