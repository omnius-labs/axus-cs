using Omnius.Core.Cryptography;

namespace Omnius.Axus.Engines.Internal.Models;

internal record SubscribedFileItem
{
    public SubscribedFileItem(OmniHash rootHash, string registrant)
    {
        this.RootHash = rootHash;
        this.Registrant = registrant;
    }

    public OmniHash RootHash { get; }

    public string Registrant { get; }
}
