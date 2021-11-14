using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Omnius.Xeus.Intaractors;
using Omnius.Xeus.Service.Models;
using Omnius.Xeus.Ui.Desktop.ViewModels;
using Omnius.Xeus.Ui.Desktop.Windows;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.Controls;

public class PeersControlViewModel : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IDashboard _dashboard;
    private readonly IApplicationDispatcher _applicationDispatcher;
    private readonly IDialogService _dialogService;

    private readonly CollectionViewUpdater<SessionViewModel, SessionReport> _sessionsUpdater;

    private readonly CompositeDisposable _disposable = new();

    public PeersControlViewModel(IDashboard dashboard, IApplicationDispatcher applicationDispatcher, IDialogService dialogService)
    {
        _dashboard = dashboard;
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

    private async ValueTask<IEnumerable<SessionReport>> GetSessionReports()
    {
        return await _dashboard.GetSessionsReportAsync();
    }

    private class SessionReportEqualityComparer : IEqualityComparer<SessionReport>
    {
        public static SessionReportEqualityComparer Default { get; } = new();

        public bool Equals(SessionReport? x, SessionReport? y)
        {
            return (x?.Address == y?.Address);
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
        var text = await _dialogService.GetTextWindowAsync();
        await _dashboard.AddCloudNodeLocationsAsync(ParseNodeLocations(text));
    }

    private static IEnumerable<NodeLocation> ParseNodeLocations(string text)
    {
        var results = new List<NodeLocation>();

        foreach (var line in text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(n => n.Trim()))
        {
            if (!XeusMessage.TryStringToNodeLocation(line, out var nodeLocation)) continue;
            results.Add(nodeLocation);
        }

        return results;
    }
}
