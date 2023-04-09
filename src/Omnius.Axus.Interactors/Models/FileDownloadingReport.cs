namespace Omnius.Axus.Interactors.Models;

public record FileDownloadingReport
{
    public FileDownloadingReport(Seed seed, string? filePath, DateTime createdTime, FileDownloadingStatus status)
    {
        this.Seed = seed;
        this.FilePath = filePath;
        this.CreatedTime = createdTime;
        this.Status = status;
    }

    public Seed Seed { get; }
    public string? FilePath { get; }
    public DateTime CreatedTime { get; }
    public FileDownloadingStatus Status { get; }
}

public record FileDownloadingStatus
{
    public FileDownloadingStatus(int currentDepth, uint downloadedBlockCount, uint totalBlockCount, FileDownloadingState State)
    {
        this.CurrentDepth = currentDepth;
        this.DownloadedBlockCount = downloadedBlockCount;
        this.TotalBlockCount = totalBlockCount;
        this.State = State;
    }

    public int CurrentDepth { get; }
    public uint DownloadedBlockCount { get; }
    public uint TotalBlockCount { get; }
    public FileDownloadingState State { get; }
}

public enum FileDownloadingState : byte
{
    Unknown = 0,
    Downloading = 1,
    Decoding = 2,
    Completed = 3,
}
