using Omnius.Axus.Engines.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record ShoutPublishedItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }
    public string? Channel { get; set; }
    public IReadOnlyList<string>? Authors { get; set; }
    public DateTime ShoutUpdatedTime { get; set; }

    public static ShoutPublishedItemEntity Import(ShoutPublishedItem item)
    {
        return new ShoutPublishedItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            Channel = item.Channel,
            Authors = item.Authors,
            ShoutUpdatedTime = item.ShoutUpdatedTime,
        };
    }

    public ShoutPublishedItem Export()
    {
        return new ShoutPublishedItem()
        {
            Signature = this.Signature?.Export() ?? OmniSignature.Empty,
            Channel = this.Channel ?? string.Empty,
            Authors = this.Authors ?? Array.Empty<string>(),
            ShoutUpdatedTime = this.ShoutUpdatedTime,
        };
    }
}
