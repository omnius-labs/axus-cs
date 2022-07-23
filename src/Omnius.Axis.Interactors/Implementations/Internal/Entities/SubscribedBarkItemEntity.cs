using Omnius.Axis.Interactors.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axis.Interactors.Internal.Entities;

internal record SubscribedBarkItemEntity
{
    public string? Tag { get; set; }

    public OmniSignatureEntity? Signature { get; set; }

    public string? Comment { get; set; }

    public OmniHashEntity? SelfHash { get; set; }

    public OmniHashEntity? ReplyHash { get; set; }

    public DateTime MessageCreatedTime { get; set; }

    public DateTime PackageCreatedTime { get; set; }

    public static SubscribedBarkItemEntity Import(SubscribedBarkItem item)
    {
        return new SubscribedBarkItemEntity()
        {
            Tag = item.Tag,
            Signature = OmniSignatureEntity.Import(item.Signature),
            Comment = item.Comment,
            SelfHash = OmniHashEntity.Import(item.SelfHash),
            ReplyHash = item.ReplyHash is null ? null : OmniHashEntity.Import(item.ReplyHash.Value),
            MessageCreatedTime = item.MessageCreatedTime,
            PackageCreatedTime = item.PackageCreatedTime,
        };
    }

    public SubscribedBarkItem Export()
    {
        return new SubscribedBarkItem(this.Tag ?? string.Empty, this.Signature?.Export() ?? OmniSignature.Empty, this.Comment ?? string.Empty, this.SelfHash?.Export() ?? OmniHash.Empty, this.ReplyHash?.Export() ?? OmniHash.Empty, this.MessageCreatedTime, this.PackageCreatedTime);
    }
}
