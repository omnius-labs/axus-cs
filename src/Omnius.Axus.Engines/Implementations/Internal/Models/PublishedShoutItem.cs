using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal record PublishedShoutItem
{
    public PublishedShoutItem(OmniSignature signature, string channel, DateTime createdTime, string registrant)
    {
        this.Signature = signature;
        this.Channel = channel;
        this.CreatedTime = createdTime;
        this.Registrant = registrant;
    }

    public OmniSignature Signature { get; }
    public string Channel { get; }
    public DateTime CreatedTime { get; }
    public string Registrant { get; }
}
