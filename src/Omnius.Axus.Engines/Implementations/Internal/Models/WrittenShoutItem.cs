using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal record WrittenShoutItem
{
    public WrittenShoutItem(OmniSignature signature, string channel, DateTime createdTime)
    {
        this.Signature = signature;
        this.Channel = channel;
        this.CreatedTime = createdTime;
    }

    public OmniSignature Signature { get; }
    public string Channel { get; }
    public DateTime CreatedTime { get; }
}
