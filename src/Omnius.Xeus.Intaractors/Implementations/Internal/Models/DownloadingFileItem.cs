using Omnius.Xeus.Intaractors.Models;

namespace Omnius.Xeus.Intaractors.Internal.Models;

internal record DownloadingFileItem
{
    public DownloadingFileItem(Seed seed, string? filePath, DateTime creationTime, DownloadingFileState state)
    {
        this.Seed = seed;
        this.FilePath = filePath;
        this.CreationTime = creationTime;
        this.State = state;
    }

    public Seed Seed { get; }

    public string? FilePath { get; }

    public DateTime CreationTime { get; }

    public DownloadingFileState State { get; }
}
