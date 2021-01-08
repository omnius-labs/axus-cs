using System;
using Omnius.Xeus.Engines.Storages.Internal.Models;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities
{
    internal sealed class DeclaredMessagePublisherItemEntity
    {
        public int Id { get; set; }

        public OmniSignatureEntity? Signature { get; set; }

        public string? Registrant { get; set; }

        public static DeclaredMessagePublisherItemEntity Import(DeclaredMessagePublisherItem value)
        {
            return new DeclaredMessagePublisherItemEntity() { Signature = OmniSignatureEntity.Import(value.Signature), Registrant = value.Registrant };
        }

        public DeclaredMessagePublisherItem Export()
        {
            return new DeclaredMessagePublisherItem(this.Signature!.Export(), this.Registrant ?? string.Empty);
        }
    }
}
