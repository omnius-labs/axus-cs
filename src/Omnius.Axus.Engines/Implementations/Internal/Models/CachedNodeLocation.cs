using Omnius.Axus.Models;

namespace Omnius.Axus.Engines.Internal.Models;

internal record CachedNodeLocation
{
    public NodeLocation Value { get; init; } = NodeLocation.Empty;
    public DateTime LastConnectedTime { get; init; }
    public DateTime UpdatedTime { get; init; }
}
