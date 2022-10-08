using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal record PublishedInternalBlockItem
{
    public OmniHash RootHash { get; init; }
    public OmniHash BlockHash { get; init; }
    public int Depth { get; init; }
    public int Order { get; init; }
}
