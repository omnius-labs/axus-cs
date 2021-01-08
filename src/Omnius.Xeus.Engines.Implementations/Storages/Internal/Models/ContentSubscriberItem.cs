using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Engines.Storages.Internal.Models
{
    internal sealed class ContentSubscriberItem
    {
        public ContentSubscriberItem(OmniHash contentHash, string registrant)
        {
            this.ContentHash = contentHash;
            this.Registrant = registrant;
        }

        public OmniHash ContentHash { get; }

        public string Registrant { get; }
    }
}
