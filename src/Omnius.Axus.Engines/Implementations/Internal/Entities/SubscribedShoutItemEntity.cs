using Omnius.Axus.Engines.Internal.Models;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record SubscribedShoutItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }

    public string? Registrant { get; set; }

    public static SubscribedShoutItemEntity Import(SubscribedShoutItem item)
    {
        return new SubscribedShoutItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            Registrant = item.Registrant,
        };
    }

    public SubscribedShoutItem Export()
    {
        return new SubscribedShoutItem(this.Signature!.Export(), this.Registrant ?? string.Empty);
    }
}
