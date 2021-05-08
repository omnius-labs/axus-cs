using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Storages.Internal.Models;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities
{
    internal record SubscribedContentItemEntity
    {
        public int Id { get; set; }

        public OmniHashEntity? RootHash { get; set; }

        public string? Registrant { get; set; }

        public static SubscribedContentItemEntity Import(SubscribedContentItem value)
        {
            return new SubscribedContentItemEntity() { RootHash = OmniHashEntity.Import(value.RootHash), Registrant = value.Registrant };
        }

        public SubscribedContentItem Export()
        {
            return new SubscribedContentItem(this.RootHash?.Export() ?? OmniHash.Empty, this.Registrant ?? string.Empty);
        }
    }
}
