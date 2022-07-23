using Omnius.Axis.Engines.Internal.Models;

namespace Omnius.Axis.Engines.Internal.Entities;

internal record PublishedShoutItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }

    public DateTime CreatedTime { get; set; }

    public string? Registrant { get; set; }

    public static PublishedShoutItemEntity Import(PublishedShoutItem item)
    {
        return new PublishedShoutItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            CreatedTime = item.CreatedTime,
            Registrant = item.Registrant,
        };
    }

    public PublishedShoutItem Export()
    {
        return new PublishedShoutItem(this.Signature!.Export(), this.CreatedTime, this.Registrant ?? string.Empty);
    }
}
