using Omnius.Core.Cryptography;

namespace Omnius.Axis.Intaractors.Internal.Models;

internal record SubscribedProfileItem
{
    public SubscribedProfileItem(OmniSignature signature, OmniHash rootHash, DateTime creationTime)
    {
        this.Signature = signature;
        this.RootHash = rootHash;
        this.CreationTime = creationTime;
    }

    public OmniSignature Signature { get; }

    public OmniHash RootHash { get; }

    public DateTime CreationTime { get; }
}
