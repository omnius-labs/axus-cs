using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors.Internal.Models;

internal record DownloadingFileItem
{
    public Seed Seed { get; init; } = Seed.Empty;
    public string? FilePath { get; init; }
    public DownloadingFileState State { get; init; }
    public DateTime CreatedTime { get; init; }
}
