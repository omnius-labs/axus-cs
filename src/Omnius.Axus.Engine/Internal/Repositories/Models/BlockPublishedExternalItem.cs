using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engine.Internal.Repositories.Models;

internal record BlockPublishedExternalItem
{
    public required string FilePath { get; init; }
    public required OmniHash RootHash { get; init; }
    public required OmniHash BlockHash { get; init; }
    public required int Index { get; init; }
    public required long Offset { get; init; }
    public required int Length { get; init; }
}
