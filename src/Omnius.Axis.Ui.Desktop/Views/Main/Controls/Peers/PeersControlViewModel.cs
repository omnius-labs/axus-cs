using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using Omnius.Axis.Intaractors;
using Omnius.Axis.Models;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Axis.Ui.Desktop.ViewModels;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axis.Ui.Desktop.Views.Main;

public class PeersControlViewModel : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IIntaractorProvider _intaractorAdapter;
    private readonly IApplicationDispatcher _applicationDispatcher;
    private readonly IDialogService _dialogService;

    private readonly CollectionViewUpdater<SessionViewModel, SessionReport> _sessionsUpdater;

    private readonly CompositeDisposable _disposable = new();

    public PeersControlViewModel(IIntaractorProvider intaractorAdapter, IApplicationDispatcher applicationDispatcher, IDialogService dialogService)
    {
        _intaractorAdapter = intaractorAdapter;
        _applicationDispatcher = applicationDispatcher;
        _dialogService = dialogService;

        _sessionsUpdater = new CollectionViewUpdater<SessionViewModel, SessionReport>(_applicationDispatcher, this.GetSessionReports, TimeSpan.FromSeconds(3), SessionReportEqualityComparer.Default);

        this.AddNodeCommand = new ReactiveCommand().AddTo(_disposable);
        this.AddNodeCommand.Subscribe(() => this.AddNodeLocations()).AddTo(_disposable);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();
        await _sessionsUpdater.DisposeAsync();
    }

    private async ValueTask<IEnumerable<SessionReport>> GetSessionReports(CancellationToken cancellationToken)
    {
        var serviceAdapter = await _intaractorAdapter.GetServiceAdapterAsync(cancellationToken);

        return await serviceAdapter.GetSessionReportsAsync(cancellationToken);
    }

    private class SessionReportEqualityComparer : IEqualityComparer<SessionReport>
    {
        public static SessionReportEqualityComparer Default { get; } = new();

        public bool Equals(SessionReport? x, SessionReport? y)
        {
            return (x?.ServiceName == y?.ServiceName) && (x?.HandshakeType == y?.HandshakeType) && (x?.Address == y?.Address);
        }

        public int GetHashCode([DisallowNull] SessionReport obj)
        {
            return obj?.Address?.GetHashCode() ?? 0;
        }
    }

    public ReactiveCommand AddNodeCommand { get; }

    public ReadOnlyObservableCollection<SessionViewModel> SessionReports => _sessionsUpdater.Collection;

    private async void AddNodeLocations()
    {
        var serviceAdapter = await _intaractorAdapter.GetServiceAdapterAsync();

        var text = await _dialogService.ShowMultiLineTextBoxWindowAsync();
        await serviceAdapter.AddCloudNodeLocationsAsync(ParseNodeLocations(text));
    }

    private static IEnumerable<NodeLocation> ParseNodeLocations(string text)
    {
        var results = new List<NodeLocation>();

        foreach (var line in text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(n => n.Trim()))
        {
            if (!AxisMessage.TryStringToNode(line, out var nodeLocation)) continue;
            results.Add(nodeLocation);
        }

        return results;
    }
}
