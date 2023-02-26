using Omnius.Axus.Engines.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record BlockSubscribedItemEntity
{
    public OmniHashEntity? RootHash { get; set; }
    public OmniHashEntity? BlockHash { get; set; }
    public int Depth { get; set; }
    public int Order { get; set; }
    public bool IsDownloaded { get; set; }

    public static BlockSubscribedItemEntity Import(BlockSubscribedItem item)
    {
        return new BlockSubscribedItemEntity()
        {
            RootHash = OmniHashEntity.Import(item.RootHash),
            BlockHash = OmniHashEntity.Import(item.BlockHash),
            Depth = item.Depth,
            Order = item.Order,
            IsDownloaded = item.IsDownloaded,
        };
    }

    public BlockSubscribedItem Export()
    {
        return new BlockSubscribedItem()
        {
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
            BlockHash = this.BlockHash?.Export() ?? OmniHash.Empty,
            Depth = this.Depth,
            Order = this.Order,
            IsDownloaded = this.IsDownloaded,
        };
    }
}
