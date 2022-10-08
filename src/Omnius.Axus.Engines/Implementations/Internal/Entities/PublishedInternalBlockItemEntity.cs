using Omnius.Axus.Engines.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record PublishedInternalBlockItemEntity
{
    public OmniHashEntity? RootHash { get; set; }
    public OmniHashEntity? BlockHash { get; set; }
    public int Depth { get; set; }
    public int Order { get; set; }

    public static PublishedInternalBlockItemEntity Import(PublishedInternalBlockItem item)
    {
        return new PublishedInternalBlockItemEntity()
        {
            RootHash = OmniHashEntity.Import(item.RootHash),
            BlockHash = OmniHashEntity.Import(item.BlockHash),
            Depth = item.Depth,
            Order = item.Order,
        };
    }

    public PublishedInternalBlockItem Export()
    {
        return new PublishedInternalBlockItem()
        {
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
            BlockHash = this.BlockHash?.Export() ?? OmniHash.Empty,
            Depth = this.Depth,
            Order = this.Order,
        };
    }
}
