using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record SubscribedProfileItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }
    public OmniHashEntity? RootHash { get; set; }
    public DateTime ShoutUpdatedTime { get; set; }

    public static SubscribedProfileItemEntity Import(SubscribedProfileItem item)
    {
        return new SubscribedProfileItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            RootHash = OmniHashEntity.Import(item.RootHash),
            ShoutUpdatedTime = item.ShoutUpdatedTime,
        };
    }

    public SubscribedProfileItem Export()
    {
        return new SubscribedProfileItem
        {
            Signature = this.Signature?.Export() ?? OmniSignature.Empty,
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
            ShoutUpdatedTime = this.ShoutUpdatedTime,
        };
    }
}
