namespace Omnius.Axis.Intaractors.Models;

public record DownloadingFileReport
{
    public DownloadingFileReport(Seed seed, string? filePath, DateTime creationTime, DownloadingFileStatus status)
    {
        this.Seed = seed;
        this.FilePath = filePath;
        this.CreationTime = creationTime;
        this.Status = status;
    }

    public Seed Seed { get; }
    public string? FilePath { get; }
    public DateTime CreationTime { get; }
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
