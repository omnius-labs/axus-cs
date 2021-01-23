using System.Reactive.Disposables;
using Omnius.Core;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.Service.Models
{
    public sealed class FileSearchResultModel
    {
        public FileSearchResultModel(string name)
        {
            this.Name = name;
        }

        public string? Name { get; set; }
    }
}
