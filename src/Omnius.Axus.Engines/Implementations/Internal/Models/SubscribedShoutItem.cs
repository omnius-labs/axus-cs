using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal record SubscribedShoutItem
{
    public required OmniSignature Signature { get; init; }
    public required string Channel { get; init; }
    public required IReadOnlyList<string> Authors { get; init; }
    public required DateTime ShoutUpdatedTime { get; init; }
}
