using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors.Internal.Models;

internal record DownloadingFileItem
{
    public FileSeed FileSeed { get; init; } = FileSeed.Empty;
    public string? FilePath { get; init; }
    public DownloadingFileState State { get; init; }
    public DateTime CreatedTime { get; init; }
}
