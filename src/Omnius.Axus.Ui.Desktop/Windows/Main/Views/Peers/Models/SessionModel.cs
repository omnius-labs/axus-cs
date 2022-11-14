using Generator.Equals;
using Omnius.Axus.Messages;
using Omnius.Core.Avalonia;

namespace Omnius.Axus.Ui.Desktop.Windows.Main;

[Equatable]
public partial class SessionViewModel : BindableBase, ICollectionViewModel<SessionViewModel, SessionReport>
{
    private SessionReport? _model;

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

    public string Scheme => this.Model?.Scheme ?? "";

    public string HandshakeType => this.Model?.HandshakeType.ToString() ?? "";

    public string Address => this.Model?.Address.ToString() ?? "";
}
