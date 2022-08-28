using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record SubscribedBarkItemEntity
{
    public OmniSignatureEntity? Signature { get; set; }

    public OmniHashEntity? RootHash { get; set; }

    public DateTime CreatedTime { get; set; }

    public static SubscribedBarkItemEntity Import(SubscribedBarkItem item)
    {
        return new SubscribedBarkItemEntity()
        {
            Signature = OmniSignatureEntity.Import(item.Signature),
            RootHash = OmniHashEntity.Import(item.RootHash),
            CreatedTime = item.CreatedTime,
        };
    }

    public SubscribedBarkItem Export()
    {
        return new SubscribedBarkItem(this.Signature?.Export() ?? OmniSignature.Empty, this.RootHash?.Export() ?? OmniHash.Empty, this.CreatedTime);
    }
}
