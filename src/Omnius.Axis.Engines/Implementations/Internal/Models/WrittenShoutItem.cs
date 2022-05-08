using Omnius.Core.Cryptography;

namespace Omnius.Axis.Engines.Internal.Models;

internal record WrittenShoutItem
{
    public WrittenShoutItem(OmniSignature signature, DateTime createdTime)
    {
        this.Signature = signature;
        this.CreatedTime = createdTime;
    }

    public OmniSignature Signature { get; }

    public DateTime CreatedTime { get; }
}
