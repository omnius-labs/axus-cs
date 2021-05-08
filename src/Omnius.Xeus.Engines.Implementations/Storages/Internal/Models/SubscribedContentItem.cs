using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Engines.Storages.Internal.Models
{
    internal record SubscribedContentItem
    {
        public SubscribedContentItem(OmniHash rootHash, string registrant)
        {
            this.RootHash = rootHash;
            this.Registrant = registrant;
        }

        public OmniHash RootHash { get; }

        public string Registrant { get; }
    }
}
