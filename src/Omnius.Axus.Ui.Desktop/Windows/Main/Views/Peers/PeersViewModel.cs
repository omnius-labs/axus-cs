using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Omnius.Axus.Interactors;
using Omnius.Axus.Messages;
using Omnius.Axus.Ui.Desktop.Internal;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axus.Ui.Desktop.Windows.Main;

public abstract class PeersViewModelBase : AsyncDisposableBase
{
    public AsyncReactiveCommand? AddCommand { get; protected set; }

    public ReadOnlyObservableCollection<SessionViewModel>? SessionReports { get; protected set; }
}

public class PeersViewModel : PeersViewModelBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IAxusServiceMediator _serviceMediator;
    private readonly IApplicationDispatcher _applicationDispatcher;
    private readonly IDialogService _dialogService;

    private readonly CollectionViewUpdater<SessionViewModel, SessionReport> _sessionsUpdater;

    private readonly CompositeDisposable _disposable = new();

    public PeersViewModel(IAxusServiceMediator serviceMediator, IApplicationDispatcher applicationDispatcher, IDialogService dialogService)
    {
        _serviceMediator = serviceMediator;
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
        return await _serviceMediator.GetSessionReportsAsync(cancellationToken);
    }

    private class SessionReportEqualityComparer : IEqualityComparer<SessionReport>
    {
        public static SessionReportEqualityComparer Default { get; } = new();

        public bool Equals(SessionReport? x, SessionReport? y)
        {
            return (x?.Scheme == y?.Scheme) && (x?.HandshakeType == y?.HandshakeType) && (x?.Address == y?.Address);
        }

        public int GetHashCode([DisallowNull] SessionReport obj)
        {
            return obj?.Address?.GetHashCode() ?? 0;
        }
    }

    private async Task AddNodeLocationsAsync()
    {
        var text = await _dialogService.ShowMultilineTextEditAsync();
        await _serviceMediator.AddCloudNodeLocationsAsync(ParseNodeLocations(text));
    }

    private static IEnumerable<NodeLocation> ParseNodeLocations(string text)
    {
        var results = new List<NodeLocation>();

        foreach (var line in text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(n => n.Trim()))
        {
            if (!AxusUriConverter.Instance.TryStringToNodeLocation(line, out var nodeLocation)) continue;
            results.Add(nodeLocation);
        }

        return results;
    }
}
