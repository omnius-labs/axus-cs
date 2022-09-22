using Omnius.Axus.Engines.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record PublishedShoutItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }
    public string? Channel { get; set; }
    public IReadOnlyList<string>? Authors { get; set; }
    public DateTime ShoutUpdatedTime { get; set; }

    public static PublishedShoutItemEntity Import(PublishedShoutItem item)
    {
        return new PublishedShoutItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            Channel = item.Channel,
            Authors = item.Authors,
            ShoutUpdatedTime = item.ShoutUpdatedTime,
        };
    }

    public PublishedShoutItem Export()
    {
        return new PublishedShoutItem()
        {
            Signature = this.Signature?.Export() ?? OmniSignature.Empty,
            Channel = this.Channel ?? string.Empty,
            Authors = this.Authors ?? Array.Empty<string>(),
            ShoutUpdatedTime = this.ShoutUpdatedTime,
        };
    }
}
