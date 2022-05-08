using Omnius.Axis.Intaractors.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axis.Intaractors.Internal.Entities;

internal record SubscribedProfileItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }

    public OmniHashEntity? RootHash { get; set; }

    public DateTime CreatedTime { get; set; }

    public static SubscribedProfileItemEntity Import(SubscribedProfileItem value)
    {
        return new SubscribedProfileItemEntity()
        {
            Signature = OmniSignatureEntity.Import(value.Signature),
            RootHash = OmniHashEntity.Import(value.RootHash),
            CreatedTime = value.CreatedTime,
        };
    }

    public SubscribedProfileItem Export()
    {
        return new SubscribedProfileItem(this.Signature?.Export() ?? OmniSignature.Empty, this.RootHash?.Export() ?? OmniHash.Empty, this.CreatedTime);
    }
}
