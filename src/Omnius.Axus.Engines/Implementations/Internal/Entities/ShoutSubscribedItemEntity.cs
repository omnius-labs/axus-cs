using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Messages;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record ShoutSubscribedItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }
    public DateTime CreatedTime { get; set; }
    public IReadOnlyList<AttachedProperty>? Properties { get; init; }
    public string? Channel { get; set; }
    public IReadOnlyList<string>? Zones { get; set; }

    public static ShoutSubscribedItemEntity Import(ShoutSubscribedItem item)
    {
        return new ShoutSubscribedItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            CreatedTime = item.CreatedTime,
            Properties = item.Properties,
            Channel = item.Channel,
            Zones = item.Zones,
        };
    }

    public ShoutSubscribedItem Export()
    {
        return new ShoutSubscribedItem()
        {
            Signature = this.Signature?.Export() ?? OmniSignature.Empty,
            CreatedTime = this.CreatedTime,
            Properties = this.Properties ?? Array.Empty<AttachedProperty>(),
            Channel = this.Channel ?? string.Empty,
            Zones = this.Zones ?? Array.Empty<string>(),
        };
    }
}
