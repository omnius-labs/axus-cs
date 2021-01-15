using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Engines.Storages.Internal.Models
{
    internal sealed class SubscribedContentItem
    {
        public SubscribedContentItem(OmniHash contentHash, string registrant)
        {
            this.ContentHash = contentHash;
            this.Registrant = registrant;
        }

        public OmniHash ContentHash { get; }

        public string Registrant { get; }
    }
}
