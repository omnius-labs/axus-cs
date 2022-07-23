using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record UploadingFileItemEntity
{
    public string? FilePath { get; set; }

    public FileSeedEntity? FileSeed { get; set; }

    public DateTime CreatedTime { get; set; }

    public int State { get; set; }

    public static UploadingFileItemEntity Import(UploadingFileItem item)
    {
        return new UploadingFileItemEntity()
        {
            FileSeed = FileSeedEntity.Import(item.FileSeed),
            FilePath = item.FilePath,
            CreatedTime = item.CreatedTime,
            State = (int)item.State,
        };
    }

    public UploadingFileItem Export()
    {
        return new UploadingFileItem(this.FilePath ?? string.Empty, this.FileSeed?.Export() ?? Interactors.Models.FileSeed.Empty, this.CreatedTime, (UploadingFileState)this.State);
    }
}
