using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal record SubscribedShoutItem
{
    public SubscribedShoutItem(OmniSignature signature, string channel, string registrant)
    {
        this.Signature = signature;
        this.Channel = channel;
        this.Registrant = registrant;
    }

    public OmniSignature Signature { get; }
    public string Channel { get; }
    public string Registrant { get; }
}
