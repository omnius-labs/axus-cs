using Omnius.Axis.Interactors.Internal.Models;
using Omnius.Axis.Interactors.Models;

namespace Omnius.Axis.Interactors.Internal.Entities;

internal record DownloadingFileItemEntity
{
    public FileSeedEntity? FileSeed { get; set; }

    public string? FilePath { get; set; }

    public DateTime CreatedTime { get; set; }

    public int State { get; set; }

    public static DownloadingFileItemEntity Import(DownloadingFileItem value)
    {
        return new DownloadingFileItemEntity()
        {
            FileSeed = FileSeedEntity.Import(value.FileSeed),
            FilePath = value.FilePath,
            CreatedTime = value.CreatedTime,
            State = (int)value.State,
        };
    }

    public DownloadingFileItem Export()
    {
        return new DownloadingFileItem(this.FileSeed?.Export() ?? Interactors.Models.FileSeed.Empty, this.FilePath, this.CreatedTime, (DownloadingFileState)this.State);
    }
}
