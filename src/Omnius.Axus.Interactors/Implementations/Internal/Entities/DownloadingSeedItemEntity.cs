using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record DownloadingSeedItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }
    public OmniHashEntity? RootHash { get; set; }
    public DateTime ShoutUpdatedTime { get; set; }

    public static DownloadingSeedItemEntity Import(DownloadingSeedItem item)
    {
        return new DownloadingSeedItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            RootHash = OmniHashEntity.Import(item.RootHash),
            ShoutUpdatedTime = item.ShoutUpdatedTime,
        };
    }

    public DownloadingSeedItem Export()
    {
        return new DownloadingSeedItem()
        {
            Signature = this.Signature?.Export() ?? OmniSignature.Empty,
            RootHash = this.RootHash?.Export() ?? OmniHash.Empty,
            ShoutUpdatedTime = this.ShoutUpdatedTime,
        };
    }
}
