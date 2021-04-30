using Omnius.Xeus.Interactors.Models;
using Omnius.Xeus.Ui.Desktop.Models.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Models.Peers
{
    public class ConnectionReportElement : BindableBase
    {
        private ConnectionReport? _model;

        public ConnectionReportElement(ConnectionReport? model)
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
