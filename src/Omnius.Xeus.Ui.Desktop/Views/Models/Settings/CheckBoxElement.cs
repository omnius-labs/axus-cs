using Omnius.Xeus.Interactors.Models;
using Omnius.Xeus.Ui.Desktop.Views.Models.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Views.Models.Settings
{
    public class CheckBoxElement : BindableBase
    {
        private string? _title;
        private bool _value;
        private string? _description;

        public string? Title
        {
            get => _title;
            set
            {
                _title = value;
                this.RaisePropertyChanged();
            }
        }

        public bool Value
        {
            get => _value;
            set
            {
                _value = value;
                this.RaisePropertyChanged();
            }
        }

        public string? Description
        {
            get => _description;
            set
            {
                _description = value;
                this.RaisePropertyChanged();
            }
        }
    }
}
