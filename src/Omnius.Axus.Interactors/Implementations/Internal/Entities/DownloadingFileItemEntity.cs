using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record DownloadingFileItemEntity
{
    public SeedEntity? Seed { get; set; }
    public string? FilePath { get; set; }
    public int State { get; set; }
    public DateTime CreatedTime { get; set; }

    public static DownloadingFileItemEntity Import(DownloadingFileItem item)
    {
        return new DownloadingFileItemEntity()
        {
            Seed = SeedEntity.Import(item.Seed),
            FilePath = item.FilePath,
            State = (int)item.State,
            CreatedTime = item.CreatedTime,
        };
    }

    public DownloadingFileItem Export()
    {
        return new DownloadingFileItem()
        {
            Seed = this.Seed?.Export() ?? Interactors.Models.Seed.Empty,
            FilePath = this.FilePath,
            State = (DownloadingFileState)this.State,
            CreatedTime = this.CreatedTime,
        };
    }
}
