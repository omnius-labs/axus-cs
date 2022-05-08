using Omnius.Axis.Intaractors.Internal.Models;
using Omnius.Axis.Intaractors.Models;

namespace Omnius.Axis.Intaractors.Internal.Entities;

internal record DownloadingFileItemEntity
{
    public SeedEntity? Seed { get; set; }

    public string? FilePath { get; set; }

    public DateTime CreatedTime { get; set; }

    public int State { get; set; }

    public static DownloadingFileItemEntity Import(DownloadingFileItem value)
    {
        return new DownloadingFileItemEntity()
        {
            Seed = SeedEntity.Import(value.Seed),
            FilePath = value.FilePath,
            CreatedTime = value.CreatedTime,
            State = (int)value.State,
        };
    }

    public DownloadingFileItem Export()
    {
        return new DownloadingFileItem(this.Seed?.Export() ?? Intaractors.Models.Seed.Empty, this.FilePath, this.CreatedTime, (DownloadingFileState)this.State);
    }
}
