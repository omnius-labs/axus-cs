using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal record BlockPublishedExternalItem
{
    public required string FilePath { get; init; }
    public required OmniHash RootHash { get; init; }
    public required OmniHash BlockHash { get; init; }
    public required int Order { get; init; }
    public required long Offset { get; init; }
    public required int Count { get; init; }
}
