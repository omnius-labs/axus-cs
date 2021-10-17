using System;
using System.IO;
using Omnius.Xeus.Intaractors.Models;
using Omnius.Xeus.Ui.Desktop.Models.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Models
{
    public class UploadingFileElement : BindableBase, IEquatable<UploadingFileElement>
    {
        private UploadingFileReport _model = null!;

        public UploadingFileElement(UploadingFileReport model)
        {
            this.Model = model;
        }

        public override int GetHashCode()
        {
            return this.Model.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as UploadingFileElement);
        }

        public bool Equals(UploadingFileElement? other)
        {
            if (other is null) return false;
            if (object.ReferenceEquals(this, other)) return true;
            if (this.Model == other.Model) return true;

            return false;
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
