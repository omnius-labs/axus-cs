using Generator.Equals;
using Omnius.Axus.Interactors.Models;
using Omnius.Core.Avalonia;

namespace Omnius.Axus.Ui.Desktop.Windows.Main;

[Equatable]
public partial class DownloadingFileViewModel : BindableBase, ICollectionViewModel<DownloadingFileViewModel, FileDownloadingReport>
{
    private FileDownloadingReport? _model;

    public void Update(FileDownloadingReport? model)
    {
        this.Model = model;
    }

    public FileDownloadingReport? Model
    {
        get => _model;
        set
        {
            this.SetProperty(ref _model, value);
            this.RaisePropertyChanged(null);
        }
    }

    public string Name => this.Model?.Seed.Name ?? "";

    public DateTime CreatedTime => this.Model?.CreatedTime ?? DateTime.MinValue;

    public FileDownloadingState State => this.Model?.Status.State ?? FileDownloadingState.Unknown;

    public double Depth => this.Model?.Status?.CurrentDepth ?? -1;

    public double Rate => Math.Round(((double)(this.Model?.Status?.DownloadedBlockCount ?? 0) / this.Model?.Status?.TotalBlockCount ?? 1) * 100 * 100) / 100;

    public string RateText
    {
        get
        {
            var downloadedBlockCount = this.Model?.Status?.DownloadedBlockCount ?? 0;
            var totalBlockCount = this.Model?.Status?.TotalBlockCount ?? 1;
            var currentDepth = this.Model?.Status?.CurrentDepth ?? -1;
            var currentDepthText = currentDepth >= 0 ? currentDepth.ToString() : "?";

            return string.Format("[{0}/{1}] ({2})", downloadedBlockCount, totalBlockCount, currentDepthText);
        }
    }
}
