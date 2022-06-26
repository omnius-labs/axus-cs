using Omnius.Axis.Interactors.Models;

namespace Omnius.Axis.Interactors.Internal.Models;

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
