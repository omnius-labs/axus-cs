using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Omnius.Axis.Intaractors;
using Omnius.Axis.Models;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Omnius.Core.Net;
using Reactive.Bindings;

namespace Omnius.Axis.Ui.Desktop.Views.Main;

public class PeersControlDesignViewModel : PeersControlViewModelBase
{
    private ObservableCollection<SessionViewModel> _sessionReports = new();

    private readonly CompositeDisposable _disposable = new();

    public PeersControlDesignViewModel()
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
