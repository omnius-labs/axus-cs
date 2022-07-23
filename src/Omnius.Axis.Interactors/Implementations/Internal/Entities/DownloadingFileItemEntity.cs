using Omnius.Axis.Interactors.Internal.Models;
using Omnius.Axis.Interactors.Models;

namespace Omnius.Axis.Interactors.Internal.Entities;

internal record DownloadingFileItemEntity
{
    public FileSeedEntity? FileSeed { get; set; }

    public string? FilePath { get; set; }

    public DateTime CreatedTime { get; set; }

    public int State { get; set; }

    public static DownloadingFileItemEntity Import(DownloadingFileItem item)
    {
        return new DownloadingFileItemEntity()
        {
            FileSeed = FileSeedEntity.Import(item.FileSeed),
            FilePath = item.FilePath,
            CreatedTime = item.CreatedTime,
            State = (int)item.State,
        };
    }

    public DownloadingFileItem Export()
    {
        return new DownloadingFileItem(this.FileSeed?.Export() ?? Interactors.Models.FileSeed.Empty, this.FilePath, this.CreatedTime, (DownloadingFileState)this.State);
    }
}
