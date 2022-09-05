using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal record PublishedExternalBlockItem
{
    public string FilePath { get; init; } = string.Empty;
    public OmniHash RootHash { get; init; }
    public OmniHash BlockHash { get; init; }
    public int Order { get; init; }
    public long Offset { get; init; }
    public int Count { get; init; }
}
