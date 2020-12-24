using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Io;
using Omnius.Xeus.Ui.Desktop.Interactors.Models.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Interactors.Models
{
    public sealed class FileSearchResultModel : BindableBase
    {
        public FileSearchResultModel()
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
