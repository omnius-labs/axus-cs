using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record UploadingBarkItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }
    public OmniHashEntity? RootHash { get; set; }

    public static UploadingBarkItemEntity Import(UploadingBarkItem item)
    {
        return new UploadingBarkItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            RootHash = OmniHashEntity.Import(item.RootHash),
        };
    }

    public UploadingBarkItem Export()
    {
        return new UploadingBarkItem
        {
            Signature = this.Signature?.Export() ?? OmniSignature.Empty,
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
        };
    }
}
