using Omnius.Axus.Messages;

namespace Omnius.Axus.Engines.Internal.Models;

internal record CachedNodeLocation
{
    public required NodeLocation Value { get; init; }
    public required DateTime LastConnectedTime { get; init; }
    public required DateTime UpdatedTime { get; init; }
}
