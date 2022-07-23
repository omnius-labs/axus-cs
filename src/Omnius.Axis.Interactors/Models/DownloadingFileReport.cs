namespace Omnius.Axis.Interactors.Models;

public record DownloadingFileReport
{
    public DownloadingFileReport(FileSeed fileSeed, string? filePath, DateTime createdTime, DownloadingFileStatus status)
    {
        this.FileSeed = fileSeed;
        this.FilePath = filePath;
        this.CreatedTime = createdTime;
        this.Status = status;
    }

    public FileSeed FileSeed { get; }
    public string? FilePath { get; }
    public DateTime CreatedTime { get; }
    public DownloadingFileStatus Status { get; }
}

public record DownloadingFileStatus
{
    public DownloadingFileStatus(int currentDepth, uint downloadedBlockCount, uint totalBlockCount, DownloadingFileState State)
    {
        this.CurrentDepth = currentDepth;
        this.DownloadedBlockCount = downloadedBlockCount;
        this.TotalBlockCount = totalBlockCount;
        this.State = State;
    }

    public int CurrentDepth { get; }
    public uint DownloadedBlockCount { get; }
    public uint TotalBlockCount { get; }
    public DownloadingFileState State { get; }
}

public enum DownloadingFileState : byte
{
    Unknown = 0,
    Downloading = 1,
    Decoding = 2,
    Completed = 3,
}
