using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors.Internal.Models;

internal record UploadingFileItem
{
    public UploadingFileItem(string filePath, FileSeed fileSeed, DateTime createdTime, UploadingFileState state)
    {
        this.FilePath = filePath;
        this.FileSeed = fileSeed;
        this.CreatedTime = createdTime;
        this.State = state;
    }

    public string FilePath { get; }

    public FileSeed FileSeed { get; }

    public DateTime CreatedTime { get; }

    public UploadingFileState State { get; }
}
