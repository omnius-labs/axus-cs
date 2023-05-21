using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Messages;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record CachedNodeLocationEntity
{
    public NodeLocationEntity? Value { get; set; }
    public DateTime LastConnectedTime { get; set; }
    public DateTime UpdatedTime { get; set; }

    public static CachedNodeLocationEntity Import(CachedNodeLocation item)
    {
        return new CachedNodeLocationEntity()
        {
            Value = NodeLocationEntity.Import(item.Value),
            LastConnectedTime = item.LastConnectedTime,
            UpdatedTime = item.CreatedTime,
        };
    }

    public CachedNodeLocation Export()
    {
        return new CachedNodeLocation
        {
            Value = this.Value?.Export() ?? NodeLocation.Empty,
            LastConnectedTime = this.LastConnectedTime,
            CreatedTime = this.UpdatedTime,
        };
    }
}
