using Omnius.Axis.Engines.Internal.Models;

namespace Omnius.Axis.Engines.Internal.Entities;

internal record WrittenShoutItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }

    public DateTime CreatedTime { get; set; }

    public static WrittenShoutItemEntity Import(WrittenShoutItem item)
    {
        return new WrittenShoutItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            CreatedTime = item.CreatedTime,
        };
    }

    public WrittenShoutItem Export()
    {
        return new WrittenShoutItem(this.Signature!.Export(), this.CreatedTime);
    }
}
