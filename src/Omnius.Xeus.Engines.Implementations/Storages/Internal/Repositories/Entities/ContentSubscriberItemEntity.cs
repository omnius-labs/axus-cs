using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Storages.Internal.Models;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities
{
    internal sealed class ContentSubscriberItemEntity
    {
        public int Id { get; set; }

        public OmniHashEntity? ContentHash { get; set; }

        public string? Registrant { get; set; }

        public static ContentSubscriberItemEntity Import(ContentSubscriberItem value)
        {
            return new ContentSubscriberItemEntity() { ContentHash = OmniHashEntity.Import(value.ContentHash), Registrant = value.Registrant };
        }

        public ContentSubscriberItem Export()
        {
            return new ContentSubscriberItem(this.ContentHash?.Export() ?? OmniHash.Empty, this.Registrant ?? string.Empty);
        }
    }
}
