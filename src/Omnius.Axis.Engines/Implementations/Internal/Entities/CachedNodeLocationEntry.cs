using Omnius.Axis.Engines.Internal.Models;
using Omnius.Axis.Models;

namespace Omnius.Axis.Engines.Internal.Entities;

internal record CachedNodeLocationEntity
{
    public NodeLocationEntity? Value { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime LastConnectionTime { get; set; }

    public static CachedNodeLocationEntity Import(CachedNodeLocation value)
    {
        return new CachedNodeLocationEntity()
        {
            Value = NodeLocationEntity.Import(value.Value),
            CreationTime = value.CreationTime,
            LastConnectionTime = value.LastConnectionTime,
        };
    }

    public CachedNodeLocation Export()
    {
        return new CachedNodeLocation(this.Value?.Export() ?? NodeLocation.Empty, this.CreationTime, this.LastConnectionTime);
    }
}
