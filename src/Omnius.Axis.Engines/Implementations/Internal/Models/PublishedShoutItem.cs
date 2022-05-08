using Omnius.Core.Cryptography;

namespace Omnius.Axis.Engines.Internal.Models;

internal record PublishedShoutItem
{
    public PublishedShoutItem(OmniSignature signature, DateTime createdTime, string registrant)
    {
        this.Signature = signature;
        this.CreatedTime = createdTime;
        this.Registrant = registrant;
    }

    public OmniSignature Signature { get; }

    public DateTime CreatedTime { get; }

    public string Registrant { get; }
}
