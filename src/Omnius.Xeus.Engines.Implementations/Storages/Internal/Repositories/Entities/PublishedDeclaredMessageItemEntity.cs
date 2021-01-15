using System;
using Omnius.Xeus.Engines.Storages.Internal.Models;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities
{
    internal sealed class PublishedDeclaredMessageItemEntity
    {
        public int Id { get; set; }

        public OmniSignatureEntity? Signature { get; set; }

        public DateTime CreationTime { get; set; }

        public string? Registrant { get; set; }

        public static PublishedDeclaredMessageItemEntity Import(PublishedDeclaredMessageItem value)
        {
            return new PublishedDeclaredMessageItemEntity() { Signature = OmniSignatureEntity.Import(value.Signature), Registrant = value.Registrant };
        }

        public PublishedDeclaredMessageItem Export()
        {
            return new PublishedDeclaredMessageItem(this.Signature!.Export(), this.CreationTime, this.Registrant ?? string.Empty);
        }
    }
}
