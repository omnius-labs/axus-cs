using Omnius.Core.Cryptography;

namespace Omnius.Axus.Core.Engine.Repositories.Models;

internal record BlockPublishedInternalItem
{
    public required OmniHash RootHash { get; init; }
    public required OmniHash BlockHash { get; init; }
    public required int Depth { get; init; }
    public required int Index { get; init; }
}
