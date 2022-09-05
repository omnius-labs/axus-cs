using Omnius.Axus.Engines.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record PublishedExternalBlockItemEntity
{
    public string? FilePath { get; set; }
    public OmniHashEntity? RootHash { get; set; }
    public OmniHashEntity? BlockHash { get; set; }
    public int Order { get; set; }
    public long Offset { get; set; }
    public int Count { get; set; }

    public static PublishedExternalBlockItemEntity Import(PublishedExternalBlockItem item)
    {
        return new PublishedExternalBlockItemEntity()
        {
            FilePath = item.FilePath,
            RootHash = OmniHashEntity.Import(item.RootHash),
            BlockHash = OmniHashEntity.Import(item.BlockHash),
            Order = item.Order,
            Offset = item.Offset,
            Count = item.Count,
        };
    }

    public PublishedExternalBlockItem Export()
    {
        return new PublishedExternalBlockItem()
        {
            FilePath = this.FilePath ?? string.Empty,
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
            BlockHash = this.BlockHash?.Export() ?? OmniHash.Empty,
            Order = this.Order,
            Offset = this.Offset,
            Count = this.Count,
        };
    }
}
