using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record UploadingFileItemEntity
{
    public string? FilePath { get; set; }
    public SeedEntity? Seed { get; set; }
    public int State { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime UpdatedTime { get; set; }

    public static UploadingFileItemEntity Import(UploadingFileItem item)
    {
        return new UploadingFileItemEntity()
        {
            Seed = SeedEntity.Import(item.Seed),
            FilePath = item.FilePath,
            State = (int)item.State,
            CreatedTime = item.CreatedTime,
        };
    }

    public UploadingFileItem Export()
    {
        return new UploadingFileItem()
        {
            FilePath = this.FilePath ?? string.Empty,
            Seed = this.Seed?.Export() ?? Interactors.Models.Seed.Empty,
            State = (UploadingFileState)this.State,
            CreatedTime = this.CreatedTime,
        };
    }
}
