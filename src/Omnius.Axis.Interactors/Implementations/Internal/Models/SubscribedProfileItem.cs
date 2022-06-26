using Omnius.Core.Cryptography;

namespace Omnius.Axis.Interactors.Internal.Models;

internal record SubscribedProfileItem
{
    public SubscribedProfileItem(OmniSignature signature, OmniHash rootHash, DateTime createdTime)
    {
        this.Signature = signature;
        this.RootHash = rootHash;
        this.CreatedTime = createdTime;
    }

    public OmniSignature Signature { get; }

    public OmniHash RootHash { get; }

    public DateTime CreatedTime { get; }
}
