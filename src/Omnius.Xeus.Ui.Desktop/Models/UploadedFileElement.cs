using System.IO;
using Omnius.Xeus.Ui.Desktop.Models.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Models
{
    public class UploadedFileElement : BindableBase
    {
        private UploadingBoxItem _model = null!;

        public UploadedFileElement(UploadingBoxItem model)
        {
            this.Model = model;
        }

        public UploadingBoxItem Model
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
