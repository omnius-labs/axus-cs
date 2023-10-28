using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors.Internal.Models;

internal record FileUploadingItem
{
    public string FilePath { get; init; } = string.Empty;
    public Seed Seed { get; init; } = Seed.Empty;
    public string Name { get; init; } = string.Empty;
    public long Length { get; init; }
    public FileUploadingState State { get; init; }
    public DateTime CreatedTime { get; init; }
}
