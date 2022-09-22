using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record DownloadingFileItemEntity
{
    public FileSeedEntity? FileSeed { get; set; }
    public string? FilePath { get; set; }
    public int State { get; set; }
    public DateTime CreatedTime { get; set; }

    public static DownloadingFileItemEntity Import(DownloadingFileItem item)
    {
        return new DownloadingFileItemEntity()
        {
            FileSeed = FileSeedEntity.Import(item.FileSeed),
            FilePath = item.FilePath,
            State = (int)item.State,
            CreatedTime = item.CreatedTime,
        };
    }

    public DownloadingFileItem Export()
    {
        return new DownloadingFileItem()
        {
            FileSeed = this.FileSeed?.Export() ?? Interactors.Models.FileSeed.Empty,
            FilePath = this.FilePath,
            State = (DownloadingFileState)this.State,
            CreatedTime = this.CreatedTime,
        };
    }
}
