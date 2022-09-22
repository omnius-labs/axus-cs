using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal record SubscribedShoutItem
{
    public OmniSignature Signature { get; init; } = OmniSignature.Empty;
    public string Channel { get; init; } = string.Empty;
    public IReadOnlyList<string> Authors { get; init; } = Array.Empty<string>();
    public DateTime ShoutUpdatedTime { get; init; }
}
