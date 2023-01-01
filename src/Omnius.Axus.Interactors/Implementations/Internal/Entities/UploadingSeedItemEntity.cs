using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record UploadingSeedItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }
    public OmniHashEntity? RootHash { get; set; }

    public static UploadingSeedItemEntity Import(UploadingSeedItem item)
    {
        return new UploadingSeedItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            RootHash = OmniHashEntity.Import(item.RootHash),
        };
    }

    public UploadingSeedItem Export()
    {
        return new UploadingSeedItem
        {
            Signature = this.Signature?.Export() ?? OmniSignature.Empty,
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
        };
    }
}
