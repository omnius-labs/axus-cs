using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record UploadingProfileItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }
    public OmniHashEntity? RootHash { get; set; }

    public static UploadingProfileItemEntity Import(UploadingProfileItem item)
    {
        return new UploadingProfileItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            RootHash = OmniHashEntity.Import(item.RootHash),
        };
    }

    public UploadingProfileItem Export()
    {
        return new UploadingProfileItem
        {
            Signature = this.Signature?.Export() ?? OmniSignature.Empty,
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
        };
    }
}
