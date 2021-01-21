using Omnius.Xeus.Ui.Desktop.Interactors.Models.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Interactors.Models
{
    public sealed class FileFindResultModel : BindableBase
    {
        public FileFindResultModel()
        {
        }

        private string _name = string.Empty;

        public string Name
        {
            get => _name;
            set => this.SetProperty(ref _name, value);
        }
    }
}
