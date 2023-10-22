using Omnius.Axus.Engine.Internal.Models;
using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engine.Internal.Repositories.Models;

internal record ShoutPublishedItem
{
    public required OmniSignature Signature { get; init; }
    public required string Channel { get; init; }
    public required AttachedProperty? Property { get; init; }
    public required DateTime ShoutCreatedTime { get; init; }
    public required DateTime CreatedTime { get; init; }
    public required DateTime UpdatedTime { get; init; }
}
