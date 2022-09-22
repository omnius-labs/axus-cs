using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record PublishedProfileItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }
    public OmniHashEntity? RootHash { get; set; }

    public static PublishedProfileItemEntity Import(PublishedProfileItem item)
    {
        return new PublishedProfileItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            RootHash = OmniHashEntity.Import(item.RootHash),
        };
    }

    public PublishedProfileItem Export()
    {
        return new PublishedProfileItem
        {
            Signature = this.Signature?.Export() ?? OmniSignature.Empty,
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
        };
    }
}
