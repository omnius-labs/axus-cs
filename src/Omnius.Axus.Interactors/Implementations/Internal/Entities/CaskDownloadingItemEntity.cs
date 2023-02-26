using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record CaskDownloadingItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }
    public OmniHashEntity? RootHash { get; set; }
    public DateTime ShoutUpdatedTime { get; set; }

    public static CaskDownloadingItemEntity Import(CaskDownloadingItem item)
    {
        return new CaskDownloadingItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            RootHash = OmniHashEntity.Import(item.RootHash),
            ShoutUpdatedTime = item.ShoutUpdatedTime,
        };
    }

    public CaskDownloadingItem Export()
    {
        return new CaskDownloadingItem()
        {
            Signature = this.Signature?.Export() ?? OmniSignature.Empty,
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
            ShoutUpdatedTime = this.ShoutUpdatedTime,
        };
    }
}
