using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record PublishedProfileItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }

    public OmniHashEntity? RootHash { get; set; }

    public DateTime CreatedTime { get; set; }

    public static PublishedProfileItemEntity Import(PublishedProfileItem item)
    {
        return new PublishedProfileItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            RootHash = OmniHashEntity.Import(item.RootHash),
            CreatedTime = item.CreatedTime,
        };
    }

    public PublishedProfileItem Export()
    {
        return new PublishedProfileItem(this.Signature?.Export() ?? OmniSignature.Empty, this.RootHash?.Export() ?? OmniHash.Empty, this.CreatedTime);
    }
}
