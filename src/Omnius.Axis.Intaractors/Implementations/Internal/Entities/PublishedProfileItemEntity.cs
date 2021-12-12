using Omnius.Axis.Intaractors.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axis.Intaractors.Internal.Entities;

internal record PublishedProfileItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }

    public OmniHashEntity? RootHash { get; set; }

    public DateTime CreationTime { get; set; }

    public static PublishedProfileItemEntity Import(PublishedProfileItem value)
    {
        return new PublishedProfileItemEntity()
        {
            Signature = OmniSignatureEntity.Import(value.Signature),
            RootHash = OmniHashEntity.Import(value.RootHash),
            CreationTime = value.CreationTime,
        };
    }

    public PublishedProfileItem Export()
    {
        return new PublishedProfileItem(this.Signature?.Export() ?? OmniSignature.Empty, this.RootHash?.Export() ?? OmniHash.Empty, this.CreationTime);
    }
}
