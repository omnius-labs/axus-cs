using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Xeus.Services;
using Omnius.Xeus.Ui.Desktop.Models.Peers;
using Omnius.Xeus.Ui.Desktop.Models.Primitives;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.Windows.Main.Peers
{
    public class PeersControlViewModel : AsyncDisposableBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IDashboard _dashboard;
        private readonly IDialogService _dialogService;

        private readonly Task _refreshTask;

        private readonly ObservableDictionary<(string, OmniAddress), ConnectionReportElement> _connectionReportMap = new();

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly CompositeDisposable _disposable = new();

        public PeersControlViewModel(IDashboard dashboard, IDialogService dialogService)
        {
            _dashboard = dashboard;
            _dialogService = dialogService;

            _refreshTask = this.RefreshAsync(_cancellationTokenSource.Token);

            this.AddNodeCommand = new ReactiveCommand().AddTo(_disposable);
            this.AddNodeCommand.Subscribe(() => this.AddNodeProfiles());
            this.ConnectionReports = _connectionReportMap.Values.ToReadOnlyReactiveCollection().AddTo(_disposable);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await _refreshTask;
            _cancellationTokenSource.Dispose();

            _disposable.Dispose();
        }

        public ReactiveCommand AddNodeCommand { get; }

        public ReadOnlyReactiveCollection<ConnectionReportElement> ConnectionReports { get; }

        private async void AddNodeProfiles()
        {
            var nodeProfiles = await _dialogService.OpenAddNodesWindowAsync();
            await _dashboard.AddCloudNodeProfileAsync(nodeProfiles);
        }

        private async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                for (; ; )
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

                    var connectionReports = await _dashboard.GetConnectionReportsAsync(cancellationToken);
                    var elements = connectionReports.SelectMany(n => n.Connections.Select(m => new ConnectionReportElement(n.EngineName, m)))
                        .ToDictionary(n => (n.EngineName, n.Model.Address));

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        foreach (var key in _connectionReportMap.Keys.ToArray())
                        {
                            if (elements.ContainsKey(key)) continue;
                            _connectionReportMap.Remove(key);
                        }

                        foreach (var (key, element) in elements)
                        {
                            if (!_connectionReportMap.TryGetValue(key, out var viewModel))
                            {
                                _connectionReportMap.Add(key, element);
                            }
                            else
                            {
                                viewModel.Model = element.Model;
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
