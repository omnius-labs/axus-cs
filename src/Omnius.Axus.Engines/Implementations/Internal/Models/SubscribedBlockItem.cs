using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal record SubscribedBlockItem
{
    public required OmniHash RootHash { get; init; }
    public required OmniHash BlockHash { get; init; }
    public required int Depth { get; init; }
    public required int Order { get; init; }
    public required bool IsDownloaded { get; init; }
}
