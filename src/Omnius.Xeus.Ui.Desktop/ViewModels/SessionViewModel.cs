using Generator.Equals;
using Omnius.Core.Avalonia;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Ui.Desktop.ViewModels;

[Equatable]
public partial class SessionViewModel : BindableBase, ICollectionViewModel<SessionViewModel, SessionReport>
{
    private SessionReport? _model;

    public SessionViewModel() { }

    public SessionViewModel(SessionReport? model)
    {
        this.Model = model;
    }

    public void Update(SessionReport? model)
    {
        this.Model = model;
    }

    public SessionReport? Model
    {
        get => _model;
        set
        {
            this.SetProperty(ref _model, value);
            this.RaisePropertyChanged(null);
        }
    }

    public string ServiceName => this.Model?.ServiceName ?? "";

    public string HandshakeType => this.Model?.HandshakeType.ToString() ?? "";

    public string Address => this.Model?.Address.ToString() ?? "";
}
