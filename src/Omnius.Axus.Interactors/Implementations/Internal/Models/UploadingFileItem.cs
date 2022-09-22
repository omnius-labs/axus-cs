using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors.Internal.Models;

internal record UploadingFileItem
{
    public string FilePath { get; init; } = string.Empty;
    public FileSeed FileSeed { get; init; } = FileSeed.Empty;
    public string Name { get; init; } = string.Empty;
    public long Length { get; init; }
    public UploadingFileState State { get; init; }
    public DateTime CreatedTime { get; init; }
}
