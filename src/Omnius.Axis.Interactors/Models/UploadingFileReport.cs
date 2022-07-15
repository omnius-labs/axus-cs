namespace Omnius.Axis.Interactors.Models;

public record UploadingFileReport
{
    public UploadingFileReport(string filePath, FileSeed? fileSeed, DateTime createdTime, UploadingFileState state)
    {
        this.FilePath = filePath;
        this.FileSeed = fileSeed;
        this.CreatedTime = createdTime;
        this.State = state;
    }

    public string FilePath { get; }
    public FileSeed? FileSeed { get; }
    public DateTime CreatedTime { get; }
    public UploadingFileState State { get; }
}
