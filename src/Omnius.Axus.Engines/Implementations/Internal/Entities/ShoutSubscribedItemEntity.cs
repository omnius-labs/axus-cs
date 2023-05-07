using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record ShoutSubscribedItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }
    public string? Channel { get; set; }
    public IReadOnlyList<string>? Zones { get; set; }
    public DateTime ShoutUpdatedTime { get; set; }
    public IReadOnlyList<AttachedProperty>? Properties { get; init; }

    public static ShoutSubscribedItemEntity Import(ShoutSubscribedItem item)
    {
        return new ShoutSubscribedItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            Channel = item.Channel,
            Zones = item.Zones,
            ShoutUpdatedTime = item.ShoutUpdatedTime,
            Properties = item.Properties,
        };
    }

    public ShoutSubscribedItem Export()
    {
        return new ShoutSubscribedItem()
        {
            Signature = this.Signature?.Export() ?? OmniSignature.Empty,
            Channel = this.Channel ?? string.Empty,
            Zones = this.Zones ?? Array.Empty<string>(),
            ShoutUpdatedTime = this.ShoutUpdatedTime,
            Properties = this.Properties ?? Array.Empty<AttachedProperty>(),
        };
    }
}
