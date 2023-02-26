using Omnius.Axus.Engines.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record BlockPublishedInternalItemEntity
{
    public OmniHashEntity? RootHash { get; set; }
    public OmniHashEntity? BlockHash { get; set; }
    public int Depth { get; set; }
    public int Order { get; set; }

    public static BlockPublishedInternalItemEntity Import(BlockPublishedInternalItem item)
    {
        return new BlockPublishedInternalItemEntity()
        {
            RootHash = OmniHashEntity.Import(item.RootHash),
            BlockHash = OmniHashEntity.Import(item.BlockHash),
            Depth = item.Depth,
            Order = item.Order,
        };
    }

    public BlockPublishedInternalItem Export()
    {
        return new BlockPublishedInternalItem()
        {
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
            BlockHash = this.BlockHash?.Export() ?? OmniHash.Empty,
            Depth = this.Depth,
            Order = this.Order,
        };
    }
}
