using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Xeus.Intaractors;
using Omnius.Xeus.Ui.Desktop.Models;
using Omnius.Xeus.Ui.Desktop.Models.Primitives;
using Omnius.Xeus.Ui.Desktop.Windows;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.Controls
{
    public class PeersControlViewModel : AsyncDisposableBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IDashboard _dashboard;
        private readonly IDialogService _dialogService;

        private readonly Task _refreshTask;

        private readonly ObservableDictionary<(string, OmniAddress), SessionReportElement> _sessionReportMap = new();

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly CompositeDisposable _disposable = new();

        public PeersControlViewModel(IDashboard dashboard, IDialogService dialogService)
        {
            _dashboard = dashboard;
            _dialogService = dialogService;

            this.AddNodeCommand = new ReactiveCommand().AddTo(_disposable);
            _ = this.AddNodeCommand.Subscribe(() => this.AddNodeLocations()).AddTo(_disposable);
            this.SessionReports = _sessionReportMap.Values.ToReadOnlyReactiveCollection().AddTo(_disposable);

            _refreshTask = this.RefreshAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await _refreshTask;
            _cancellationTokenSource.Dispose();

            _disposable.Dispose();
        }

        public ReactiveCommand AddNodeCommand { get; }

        public ReadOnlyReactiveCollection<SessionReportElement> SessionReports { get; }

        private async void AddNodeLocations()
        {
            var nodeLocations = await _dialogService.ShowNodesWindowAsync();
            await _dashboard.AddCloudNodeLocationsAsync(nodeLocations);
        }

        private async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                for (; ; )
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

                    var sessionReports = await _dashboard.GetSessionsReportAsync(cancellationToken);
                    var elements = sessionReports.Select(n => new SessionReportElement(n))
                        .ToDictionary(n => (n.ServiceName, n.Model.Address));

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        foreach (var key in _sessionReportMap.Keys.ToArray())
                        {
                            if (elements.ContainsKey(key))
                            {
                                continue;
                            }

                            _ = _sessionReportMap.Remove(key);
                        }

                        foreach (var (key, element) in elements)
                        {
                            if (!_sessionReportMap.TryGetValue(key, out var viewModel))
                            {
                                _sessionReportMap.Add(key, element);
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
