namespace Omnius.Axus.Interactors.Models;

public record FileUploadingReport
{
    public FileUploadingReport(string filePath, Seed? seed, DateTime createdTime, FileUploadingStatus status)
    {
        this.FilePath = filePath;
        this.Seed = seed;
        this.CreatedTime = createdTime;
        this.Status = status;
    }

    public string FilePath { get; }
    public Seed? Seed { get; }
    public DateTime CreatedTime { get; }
    public FileUploadingStatus Status { get; }
}

public record FileUploadingStatus
{
    public FileUploadingStatus(FileUploadingState State)
    {
        this.State = State;
    }

    public FileUploadingState State { get; }
}

public enum FileUploadingState : byte
{
    Unknown = 0,
    Waiting = 1,
    Encoding = 2,
    Completed = 3,
}
