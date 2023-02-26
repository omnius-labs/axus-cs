using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record FileDownloadingItemEntity
{
    public SeedEntity? Seed { get; set; }
    public string? FilePath { get; set; }
    public int State { get; set; }
    public DateTime CreatedTime { get; set; }

    public static FileDownloadingItemEntity Import(FileDownloadingItem item)
    {
        return new FileDownloadingItemEntity()
        {
            Seed = SeedEntity.Import(item.Seed),
            FilePath = item.FilePath,
            State = (int)item.State,
            CreatedTime = item.CreatedTime,
        };
    }

    public FileDownloadingItem Export()
    {
        return new FileDownloadingItem()
        {
            Seed = this.Seed?.Export() ?? Interactors.Models.Seed.Empty,
            FilePath = this.FilePath,
            State = (FileDownloadingState)this.State,
            CreatedTime = this.CreatedTime,
        };
    }
}
