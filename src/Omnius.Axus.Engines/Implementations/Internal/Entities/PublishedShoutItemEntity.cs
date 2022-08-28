using Omnius.Axus.Engines.Internal.Models;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record PublishedShoutItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }
    public string? Channel { get; set; }
    public DateTime CreatedTime { get; set; }
    public string? Registrant { get; set; }

    public static PublishedShoutItemEntity Import(PublishedShoutItem item)
    {
        return new PublishedShoutItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            Channel = item.Channel,
            CreatedTime = item.CreatedTime,
            Registrant = item.Registrant,
        };
    }

    public PublishedShoutItem Export()
    {
        return new PublishedShoutItem(this.Signature!.Export(), this.Channel ?? string.Empty, this.CreatedTime, this.Registrant ?? string.Empty);
    }
}
