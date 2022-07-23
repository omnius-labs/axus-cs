using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal record SubscribedShoutItem
{
    public SubscribedShoutItem(OmniSignature signature, string registrant)
    {
        this.Signature = signature;
        this.Registrant = registrant;
    }

    public OmniSignature Signature { get; }

    public string Registrant { get; }
}
