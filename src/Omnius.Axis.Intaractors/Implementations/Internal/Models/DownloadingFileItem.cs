using Omnius.Axis.Intaractors.Models;

namespace Omnius.Axis.Intaractors.Internal.Models;

internal record DownloadingFileItem
{
    public DownloadingFileItem(Seed seed, string? filePath, DateTime createdTime, DownloadingFileState state)
    {
        this.Seed = seed;
        this.FilePath = filePath;
        this.CreatedTime = createdTime;
        this.State = state;
    }

    public Seed Seed { get; }

    public string? FilePath { get; }

    public DateTime CreatedTime { get; }

    public DownloadingFileState State { get; }
}
