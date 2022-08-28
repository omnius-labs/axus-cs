using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal record SubscribedBlockItem
{
    public OmniHash RootHash { get; init; }
    public OmniHash BlockHash { get; init; }
    public int Depth { get; init; }
    public int Order { get; init; }
    public bool IsDownloaded { get; init; }
}
