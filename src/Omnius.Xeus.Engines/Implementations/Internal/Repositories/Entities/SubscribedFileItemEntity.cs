using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Internal.Models;

namespace Omnius.Xeus.Engines.Internal.Repositories.Entities
{
    internal record SubscribedFileItemEntity
    {
        public int Id { get; set; }

        public OmniHashEntity? RootHash { get; set; }

        public string? Registrant { get; set; }

        public static SubscribedFileItemEntity Import(SubscribedFileItem value)
        {
            return new SubscribedFileItemEntity() { RootHash = OmniHashEntity.Import(value.RootHash), Registrant = value.Registrant };
        }

        public SubscribedFileItem Export()
        {
            return new SubscribedFileItem(this.RootHash?.Export() ?? OmniHash.Empty, this.Registrant ?? string.Empty);
        }
    }
}
