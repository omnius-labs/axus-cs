using Generator.Equals;
using Omnius.Axus.Interactors.Models;
using Omnius.Core.Avalonia;

namespace Omnius.Axus.Ui.Desktop.Windows.Main;

[Equatable]
public partial class UploadingFileViewModel : BindableBase, ICollectionViewModel<UploadingFileViewModel, FileUploadingReport>
{
    private FileUploadingReport? _model;

    public void Update(FileUploadingReport? model)
    {
        this.Model = model;
    }

    public FileUploadingReport? Model
    {
        get => _model;
        set
        {
            this.SetProperty(ref _model, value);
            this.RaisePropertyChanged(null);
        }
    }

    public string Name => Path.GetFileName(this.Model?.FilePath ?? "");

    public string FilePath => this.Model?.FilePath ?? "";

    public DateTime CreatedTime => this.Model?.CreatedTime ?? DateTime.MinValue;

    public FileUploadingState State => this.Model?.Status.State ?? FileUploadingState.Unknown;
}
