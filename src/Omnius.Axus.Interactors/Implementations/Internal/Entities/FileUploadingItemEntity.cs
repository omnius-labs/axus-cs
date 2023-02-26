using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record FileUploadingItemEntity
{
    public string? FilePath { get; set; }
    public SeedEntity? Seed { get; set; }
    public int State { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime UpdatedTime { get; set; }

    public static FileUploadingItemEntity Import(FileUploadingItem item)
    {
        return new FileUploadingItemEntity()
        {
            Seed = SeedEntity.Import(item.Seed),
            FilePath = item.FilePath,
            State = (int)item.State,
            CreatedTime = item.CreatedTime,
        };
    }

    public FileUploadingItem Export()
    {
        return new FileUploadingItem()
        {
            FilePath = this.FilePath ?? string.Empty,
            Seed = this.Seed?.Export() ?? Interactors.Models.Seed.Empty,
            State = (FileUploadingState)this.State,
            CreatedTime = this.CreatedTime,
        };
    }
}
