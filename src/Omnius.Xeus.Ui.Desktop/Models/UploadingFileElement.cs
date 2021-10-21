using System;
using System.IO;
using Generator.Equals;
using Omnius.Xeus.Intaractors.Models;
using Omnius.Xeus.Ui.Desktop.Models.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Models
{
    [Equatable]
    public partial class UploadingFileElement : BindableBase, IEquatable<UploadingFileElement>
    {
        private UploadingFileReport _model = null!;

        public UploadingFileElement(UploadingFileReport model)
        {
            this.Model = model;
        }

        public UploadingFileReport Model
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

        public DateTime CreationTime => this.Model.CreationTime;

        public UploadingFileState State => this.Model.State;
    }
}
