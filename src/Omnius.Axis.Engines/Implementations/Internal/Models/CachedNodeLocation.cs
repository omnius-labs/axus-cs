using Omnius.Axis.Models;

namespace Omnius.Axis.Engines.Internal.Models;

internal record CachedNodeLocation
{
    public CachedNodeLocation(NodeLocation value, DateTime createdTime, DateTime lastConnectionTime)
    {
        this.Value = value;
        this.CreatedTime = createdTime;
        this.LastConnectionTime = lastConnectionTime;
    }

    public NodeLocation Value { get; }

    public DateTime CreatedTime { get; }

    public DateTime LastConnectionTime { get; }
}
