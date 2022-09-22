using Omnius.Axus.Engines.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record SubscribedShoutItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }
    public string? Channel { get; set; }
    public IReadOnlyList<string>? Authors { get; set; }
    public DateTime ShoutUpdatedTime { get; set; }

    public static SubscribedShoutItemEntity Import(SubscribedShoutItem item)
    {
        return new SubscribedShoutItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            Channel = item.Channel,
            Authors = item.Authors,
            ShoutUpdatedTime = item.ShoutUpdatedTime,
        };
    }

    public SubscribedShoutItem Export()
    {
        return new SubscribedShoutItem()
        {
            Signature = this.Signature?.Export() ?? OmniSignature.Empty,
            Channel = this.Channel ?? string.Empty,
            Authors = this.Authors ?? Array.Empty<string>(),
            ShoutUpdatedTime = this.ShoutUpdatedTime,
        };
    }
}
