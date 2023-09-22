using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal record ShoutPublishedItem
{
    public required OmniSignature Signature { get; init; }
    public required string Channel { get; init; }
    public required IReadOnlyList<AttachedProperty> Properties { get; init; }
    public required DateTime CreatedTime { get; init; }
    public required DateTime UpdatedTime { get; init; }
}
