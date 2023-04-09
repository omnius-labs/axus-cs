using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record SeedBoxDownloadingItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }
    public OmniHashEntity? RootHash { get; set; }
    public DateTime ShoutUpdatedTime { get; set; }

    public static SeedBoxDownloadingItemEntity Import(SeedBoxDownloadingItem item)
    {
        return new SeedBoxDownloadingItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            RootHash = OmniHashEntity.Import(item.RootHash),
            ShoutUpdatedTime = item.ShoutUpdatedTime,
        };
    }

    public SeedBoxDownloadingItem Export()
    {
        return new SeedBoxDownloadingItem()
        {
            Signature = this.Signature?.Export() ?? OmniSignature.Empty,
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
            ShoutUpdatedTime = this.ShoutUpdatedTime,
        };
    }
}
