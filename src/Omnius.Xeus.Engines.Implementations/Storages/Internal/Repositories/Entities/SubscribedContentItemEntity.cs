using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Storages.Internal.Models;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities
{
    internal sealed class SubscribedContentItemEntity
    {
        public int Id { get; set; }

        public OmniHashEntity? ContentHash { get; set; }

        public string? Registrant { get; set; }

        public static SubscribedContentItemEntity Import(SubscribedContentItem value)
        {
            return new SubscribedContentItemEntity() { ContentHash = OmniHashEntity.Import(value.ContentHash), Registrant = value.Registrant };
        }

        public SubscribedContentItem Export()
        {
            return new SubscribedContentItem(this.ContentHash?.Export() ?? OmniHash.Empty, this.Registrant ?? string.Empty);
        }
    }
}
