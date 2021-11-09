using Omnius.Xeus.Service.Engines.Internal.Models;

namespace Omnius.Xeus.Service.Engines.Internal.Entities;

internal record SubscribedShoutItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }

    public string? Registrant { get; set; }

    public static SubscribedShoutItemEntity Import(SubscribedShoutItem value)
    {
        return new SubscribedShoutItemEntity()
        {
            Signature = OmniSignatureEntity.Import(value.Signature),
            Registrant = value.Registrant,
        };
    }

    public SubscribedShoutItem Export()
    {
        return new SubscribedShoutItem(this.Signature!.Export(), this.Registrant ?? string.Empty);
    }
}