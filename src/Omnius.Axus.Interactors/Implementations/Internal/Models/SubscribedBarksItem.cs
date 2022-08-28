using Omnius.Core.Cryptography;

namespace Omnius.Axus.Interactors.Internal.Models;

internal record SubscribedBarkPackageMetadata
{
    public SubscribedBarkPackageMetadata(OmniSignature signature, DateTime updatedTime)
    {
        this.Signature = signature;
        this.UpdatedTime = updatedTime;
    }

    public OmniSignature Signature { get; }

    public DateTime UpdatedTime { get; }
}
