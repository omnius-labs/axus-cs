using System;
using Omnius.Xeus.Engines.Storages.Internal.Models;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities
{
    internal sealed class PushDeclaredMessageStatusEntity
    {
        public int Id { get; set; }
        public OmniSignatureEntity? Signature { get; set; }
        public DateTime CreationTime { get; set; }

        public static PushDeclaredMessageStatusEntity Import(PushDeclaredMessageStatus value)
        {
            return new PushDeclaredMessageStatusEntity() { Signature = OmniSignatureEntity.Import(value.Signature), CreationTime = value.CreationTime };
        }

        public PushDeclaredMessageStatus Export()
        {
            return new PushDeclaredMessageStatus(this.Signature!.Export(), this.CreationTime);
        }
    }
}
