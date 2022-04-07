using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Omnius.Axis.Intaractors;
using Omnius.Axis.Models;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axis.Ui.Desktop.Views.Main;

public abstract class PeersControlViewModelBase : AsyncDisposableBase
{
    public AsyncReactiveCommand? AddCommand { get; protected set; }

    public ReadOnlyObservableCollection<SessionViewModel>? SessionReports { get; protected set; }
}

public class PeersControlViewModel : PeersControlViewModelBase
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

        this.AddCommand = new AsyncReactiveCommand().AddTo(_disposable);
        this.AddCommand.Subscribe(async () => await this.AddNodeLocationsAsync()).AddTo(_disposable);

        _sessionsUpdater = new CollectionViewUpdater<SessionViewModel, SessionReport>(_applicationDispatcher, this.GetSessionReports, TimeSpan.FromSeconds(3), SessionReportEqualityComparer.Default);
        this.SessionReports = _sessionsUpdater.Collection;
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();
        await _sessionsUpdater.DisposeAsync();
    }

    private async ValueTask<IEnumerable<SessionReport>> GetSessionReports(CancellationToken cancellationToken)
    {
        var serviceController = await _intaractorAdapter.GetServiceControllerAsync(cancellationToken);

        return await serviceController.GetSessionReportsAsync(cancellationToken);
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

    private async Task AddNodeLocationsAsync()
    {
        var serviceController = await _intaractorAdapter.GetServiceControllerAsync();

        var text = await _dialogService.ShowMultiLineTextInputWindowAsync();
        await serviceController.AddCloudNodeLocationsAsync(ParseNodeLocations(text));
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
