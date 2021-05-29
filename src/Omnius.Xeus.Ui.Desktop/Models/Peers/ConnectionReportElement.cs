using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Services.Models;
using Omnius.Xeus.Ui.Desktop.Models.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Models.Peers
{
    public class ConnectionReportElement : BindableBase
    {
        private ConnectionReport _model = null!;

        public ConnectionReportElement(string engineName, ConnectionReport model)
        {
            this.EngineName = engineName;
            this.Model = model;
        }

        public ConnectionReport Model
        {
            get => _model;
            set
            {
                _model = value;
                this.RaisePropertyChanged();
            }
        }

        public string EngineName { get; }

        public string HandshakeType => this.Model?.HandshakeType.ToString() ?? "";

        public string Address => this.Model?.Address.ToString() ?? "";
    }
}
