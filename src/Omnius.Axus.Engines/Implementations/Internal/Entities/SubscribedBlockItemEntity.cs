using Omnius.Axus.Engines.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record SubscribedBlockItemEntity
{
    public OmniHashEntity? RootHash { get; set; }
    public OmniHashEntity? BlockHash { get; set; }
    public int Depth { get; set; }
    public int Order { get; set; }
    public bool IsDownloaded { get; set; }

    public static SubscribedBlockItemEntity Import(SubscribedBlockItem item)
    {
        return new SubscribedBlockItemEntity()
        {
            RootHash = OmniHashEntity.Import(item.RootHash),
            BlockHash = OmniHashEntity.Import(item.BlockHash),
            Depth = item.Depth,
            Order = item.Order,
            IsDownloaded = item.IsDownloaded,
        };
    }

    public SubscribedBlockItem Export()
    {
        return new SubscribedBlockItem()
        {
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
            BlockHash = this.BlockHash?.Export() ?? OmniHash.Empty,
            Depth = this.Depth,
            Order = this.Order,
            IsDownloaded = this.IsDownloaded,
        };
    }
}
