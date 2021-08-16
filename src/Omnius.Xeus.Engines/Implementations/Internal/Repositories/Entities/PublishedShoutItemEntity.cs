using System;
using Omnius.Xeus.Engines.Internal.Models;

namespace Omnius.Xeus.Engines.Internal.Repositories.Entities
{
    internal record PublishedShoutItemEntity
    {
        public int Id { get; set; }

        public OmniSignatureEntity? Signature { get; set; }

        public DateTime CreationTime { get; set; }

        public string? Registrant { get; set; }

        public static PublishedShoutItemEntity Import(PublishedShoutItem value)
        {
            return new PublishedShoutItemEntity() { Signature = OmniSignatureEntity.Import(value.Signature), Registrant = value.Registrant };
        }

        public PublishedShoutItem Export()
        {
            return new PublishedShoutItem(this.Signature!.Export(), this.CreationTime, this.Registrant ?? string.Empty);
        }
    }
}
