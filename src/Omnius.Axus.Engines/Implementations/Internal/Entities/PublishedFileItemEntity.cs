using Omnius.Axus.Engines.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record PublishedFileItemEntity
{
    public OmniHashEntity? RootHash { get; set; }
    public string? FilePath { get; set; }
    public int MaxBlockSize { get; set; }
    public IReadOnlyList<string>? Authors { get; set; }

    public static PublishedFileItemEntity Import(PublishedFileItem item)
    {
        return new PublishedFileItemEntity()
        {
            RootHash = OmniHashEntity.Import(item.RootHash),
            FilePath = item.FilePath,
            MaxBlockSize = item.MaxBlockSize,
            Authors = item.Authors,
        };
    }

    public PublishedFileItem Export()
    {
        return new PublishedFileItem()
        {
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
            FilePath = this.FilePath,
            MaxBlockSize = this.MaxBlockSize,
            Authors = this.Authors ?? Array.Empty<string>(),
        };
    }
}
