using Omnius.Axis.Interactors.Internal.Models;
using Omnius.Axis.Interactors.Models;

namespace Omnius.Axis.Interactors.Internal.Entities;

internal record UploadingFileItemEntity
{
    public string? FilePath { get; set; }

    public FileSeedEntity? FileSeed { get; set; }

    public DateTime CreatedTime { get; set; }

    public int State { get; set; }

    public static UploadingFileItemEntity Import(UploadingFileItem value)
    {
        return new UploadingFileItemEntity()
        {
            FileSeed = FileSeedEntity.Import(value.FileSeed),
            FilePath = value.FilePath,
            CreatedTime = value.CreatedTime,
            State = (int)value.State,
        };
    }

    public UploadingFileItem Export()
    {
        return new UploadingFileItem(this.FilePath ?? string.Empty, this.FileSeed?.Export() ?? Interactors.Models.FileSeed.Empty, this.CreatedTime, (UploadingFileState)this.State);
    }
}
