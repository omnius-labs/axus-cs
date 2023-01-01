using System.Collections.ObjectModel;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Net;

namespace Omnius.Axus.Ui.Desktop.Windows.Main;

public class PeersViewDesignViewModel : PeersViewModelBase
{
    private ObservableCollection<SessionViewModel> _sessionReports = new();

    private readonly CompositeDisposable _disposable = new();

    public PeersViewDesignViewModel()
    {
        _sessionReports.Add(new SessionViewModel() { Model = new SessionReport("test_service_1", SessionHandshakeType.Connected, OmniAddress.Parse("tcp(127.0.0.1,1000)")) });
        _sessionReports.Add(new SessionViewModel() { Model = new SessionReport("test_service_2", SessionHandshakeType.Accepted, OmniAddress.Parse("tcp(127.0.0.1,2000)")) });
        this.SessionReports = new ReadOnlyObservableCollection<SessionViewModel>(_sessionReports);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();
    }
}
