using System;
using Omnius.Xeus.Intaractors.Models;
using Omnius.Xeus.Ui.Desktop.Models.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Models
{
    public class DownloadingFileElement : BindableBase, IEquatable<DownloadingFileElement>
    {
        private DownloadingFileReport _model = null!;

        public DownloadingFileElement(DownloadingFileReport model)
        {
            this.Model = model;
        }

        public override int GetHashCode()
        {
            return this.Model.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as DownloadingFileElement);
        }

        public bool Equals(DownloadingFileElement? other)
        {
            if (other is null) return false;
            if (object.ReferenceEquals(this, other)) return true;
            if (this.Model == other.Model) return true;

            return false;
        }

        public DownloadingFileReport Model
        {
            get => _model;
            set
            {
                _model = value;
                this.RaisePropertyChanged();
            }
        }

        public string Name => this.Model.Seed.Name;

        public DateTime CreationTime => this.Model.CreationTime;

        public DownloadingFileState State => this.Model.State;
    }
}
