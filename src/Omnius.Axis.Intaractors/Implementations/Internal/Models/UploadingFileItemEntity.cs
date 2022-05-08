using Omnius.Axis.Intaractors.Models;

namespace Omnius.Axis.Intaractors.Internal.Models;

internal record UploadingFileItem
{
    public UploadingFileItem(string filePath, Seed seed, DateTime createdTime, UploadingFileState state)
    {
        this.FilePath = filePath;
        this.Seed = seed;
        this.CreatedTime = createdTime;
        this.State = state;
    }

    public string FilePath { get; }

    public Seed Seed { get; }

    public DateTime CreatedTime { get; }

    public UploadingFileState State { get; }
}
