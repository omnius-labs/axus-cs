namespace Omnius.Axus.Interactors.Models;

public record UploadingFileReport
{
    public UploadingFileReport(string filePath, Seed? seed, DateTime createdTime, UploadingFileStatus status)
    {
        this.FilePath = filePath;
        this.Seed = seed;
        this.CreatedTime = createdTime;
        this.Status = status;
    }

    public string FilePath { get; }
    public Seed? Seed { get; }
    public DateTime CreatedTime { get; }
    public UploadingFileStatus Status { get; }
}

public record UploadingFileStatus
{
    public UploadingFileStatus(UploadingFileState State)
    {
        this.State = State;
    }

    public UploadingFileState State { get; }
}

public enum UploadingFileState : byte
{
    Unknown = 0,
    Waiting = 1,
    Encoding = 2,
    Completed = 3,
}
