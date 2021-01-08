using System;
using Omnius.Xeus.Engines.Storages.Internal.Models;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities
{
    internal sealed class DeclaredMessageSubscriberItemEntity
    {
        public int Id { get; set; }

        public OmniSignatureEntity? Signature { get; set; }

        public string? Registrant { get; set; }

        public static DeclaredMessageSubscriberItemEntity Import(DeclaredMessageSubscriberItem value)
        {
            return new DeclaredMessageSubscriberItemEntity() { Signature = OmniSignatureEntity.Import(value.Signature), Registrant = value.Registrant };
        }

        public DeclaredMessageSubscriberItem Export()
        {
            return new DeclaredMessageSubscriberItem(this.Signature!.Export(), this.Registrant ?? string.Empty);
        }
    }
}
