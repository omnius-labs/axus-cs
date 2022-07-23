using Omnius.Axis.Interactors.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axis.Interactors.Internal.Entities;

internal record SubscribedProfileItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }

    public OmniHashEntity? RootHash { get; set; }

    public DateTime CreatedTime { get; set; }

    public static SubscribedProfileItemEntity Import(SubscribedProfileItem item)
    {
        return new SubscribedProfileItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            RootHash = OmniHashEntity.Import(item.RootHash),
            CreatedTime = item.CreatedTime,
        };
    }

    public SubscribedProfileItem Export()
    {
        return new SubscribedProfileItem(this.Signature?.Export() ?? OmniSignature.Empty, this.RootHash?.Export() ?? OmniHash.Empty, this.CreatedTime);
    }
}
