using System;
using Omnius.Xeus.Engines.Storages.Internal.Models;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities
{
    internal sealed class WantDeclaredMessageStatusEntity
    {
        public int Id { get; set; }

        public OmniSignatureEntity? Signature { get; set; }

        public DateTime CreationTime { get; set; }

        public static WantDeclaredMessageStatusEntity Import(WantDeclaredMessageStatus value)
        {
            return new WantDeclaredMessageStatusEntity() { Signature = OmniSignatureEntity.Import(value.Signature), CreationTime = value.CreationTime };
        }

        public WantDeclaredMessageStatus Export()
        {
            return new WantDeclaredMessageStatus(this.Signature!.Export(), this.CreationTime);
        }
    }
}
