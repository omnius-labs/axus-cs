using System.IO;
using Omnius.Xeus.Intaractors;
using Omnius.Xeus.Ui.Desktop.Models.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Models
{
    public class UploadedFileElement : BindableBase
    {
        private UploadedFileReport _model = null!;

        public UploadedFileElement(UploadedFileReport model)
        {
            this.Model = model;
        }

        public UploadedFileReport Model
        {
            get => _model;
            set
            {
                _model = value;
                this.RaisePropertyChanged();
            }
        }

        public string FilePath => this.Model?.FilePath ?? "";

        public string Name => Path.GetFileName(this.FilePath);
    }
}
