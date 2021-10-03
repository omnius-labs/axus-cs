using System;
using Omnius.Xeus.Intaractors.Models;
using Omnius.Xeus.Ui.Desktop.Models.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Models
{
    public class DownloadingFileElement : BindableBase
    {
        private DownloadingFileReport _model = null!;

        public DownloadingFileElement(DownloadingFileReport model)
        {
            this.Model = model;
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
