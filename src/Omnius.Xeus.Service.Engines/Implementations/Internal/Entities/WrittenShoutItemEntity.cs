using Omnius.Xeus.Service.Engines.Internal.Models;

namespace Omnius.Xeus.Service.Engines.Internal.Entities;

internal record WrittenShoutItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }

    public DateTime CreationTime { get; set; }

    public static WrittenShoutItemEntity Import(WrittenShoutItem value)
    {
        return new WrittenShoutItemEntity()
        {
            Signature = OmniSignatureEntity.Import(value.Signature),
            CreationTime = value.CreationTime,
        };
    }

    public WrittenShoutItem Export()
    {
        return new WrittenShoutItem(this.Signature!.Export(), this.CreationTime);
    }
}
