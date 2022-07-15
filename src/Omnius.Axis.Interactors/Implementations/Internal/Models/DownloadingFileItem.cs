using Omnius.Axis.Interactors.Models;

namespace Omnius.Axis.Interactors.Internal.Models;

internal record DownloadingFileItem
{
    public DownloadingFileItem(FileSeed fileSeed, string? filePath, DateTime createdTime, DownloadingFileState state)
    {
        this.FileSeed = fileSeed;
        this.FilePath = filePath;
        this.CreatedTime = createdTime;
        this.State = state;
    }

    public FileSeed FileSeed { get; }

    public string? FilePath { get; }

    public DateTime CreatedTime { get; }

    public DownloadingFileState State { get; }
}
