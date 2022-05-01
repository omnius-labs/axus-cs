using Omnius.Axis.Models;

namespace Omnius.Axis.Engines.Internal.Models;

internal record CachedNodeLocation
{
    public CachedNodeLocation(NodeLocation value, DateTime creationTime, DateTime lastConnectionTime)
    {
        this.Value = value;
        this.CreationTime = creationTime;
        this.LastConnectionTime = lastConnectionTime;
    }

    public NodeLocation Value { get; }

    public DateTime CreationTime { get; }

    public DateTime LastConnectionTime { get; }
}
