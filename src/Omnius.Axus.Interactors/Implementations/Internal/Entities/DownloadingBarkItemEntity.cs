using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record DownloadingBarkItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }
    public OmniHashEntity? RootHash { get; set; }
    public DateTime ShoutUpdatedTime { get; set; }

    public static DownloadingBarkItemEntity Import(DownloadingBarkItem item)
    {
        return new DownloadingBarkItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            RootHash = OmniHashEntity.Import(item.RootHash),
            ShoutUpdatedTime = item.ShoutUpdatedTime,
        };
    }

    public DownloadingBarkItem Export()
    {
        return new DownloadingBarkItem()
        {
            Signature = this.Signature?.Export() ?? OmniSignature.Empty,
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
            ShoutUpdatedTime = this.ShoutUpdatedTime,
        };
    }
}
