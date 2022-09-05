using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal record PublishedFileItem
{
    public OmniHash RootHash { get; init; }
    public string? FilePath { get; init; }
    public int MaxBlockSize { get; init; }
    public IReadOnlyList<string> Authors { get; init; } = Array.Empty<string>();
}
