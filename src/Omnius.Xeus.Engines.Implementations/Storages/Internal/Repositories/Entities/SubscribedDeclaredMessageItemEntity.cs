using System;
using Omnius.Xeus.Engines.Storages.Internal.Models;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities
{
    internal sealed class SubscribedDeclaredMessageItemEntity
    {
        public int Id { get; set; }

        public OmniSignatureEntity? Signature { get; set; }

        public string? Registrant { get; set; }

        public static SubscribedDeclaredMessageItemEntity Import(SubscribedDeclaredMessageItem value)
        {
            return new SubscribedDeclaredMessageItemEntity() { Signature = OmniSignatureEntity.Import(value.Signature), Registrant = value.Registrant };
        }

        public SubscribedDeclaredMessageItem Export()
        {
            return new SubscribedDeclaredMessageItem(this.Signature!.Export(), this.Registrant ?? string.Empty);
        }
    }
}
