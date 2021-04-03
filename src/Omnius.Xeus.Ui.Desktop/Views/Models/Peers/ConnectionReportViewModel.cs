using Omnius.Xeus.Interactors.Models;
using Omnius.Xeus.Ui.Desktop.Views.Models.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Views.Models.Peers
{
    public class ConnectionReportViewModel : BindableBase
    {
        private ConnectionReport? _model;

        public ConnectionReportViewModel(ConnectionReport? model)
        {
            this.Model = model;
        }

        public ConnectionReport? Model
        {
            get => _model;
            set
            {
                _model = value;
                this.RaisePropertyChanged();
            }
        }

        public string EngineName => this.Model?.EngineName ?? "";

        public string HandshakeType => this.Model?.HandshakeType.ToString() ?? "";

        public string Address => this.Model?.Address.ToString() ?? "";
    }
}
