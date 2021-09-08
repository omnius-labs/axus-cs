using Omnius.Xeus.Service.Models;
using Omnius.Xeus.Ui.Desktop.Models.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Models
{
    public class SessionReportElement : BindableBase
    {
        private SessionReport _model = null!;

        public SessionReportElement(SessionReport model)
        {
            this.Model = model;
        }

        public SessionReport Model
        {
            get => _model;
            set
            {
                _model = value;
                this.RaisePropertyChanged();
            }
        }

        public string ServiceName => this.Model?.ServiceName ?? "";

        public string HandshakeType => this.Model?.HandshakeType.ToString() ?? "";

        public string Address => this.Model?.Address.ToString() ?? "";
    }
}
