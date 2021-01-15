using System;
using Omnius.Xeus.Engines.Storages.Internal.Models;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities
{
    internal sealed class WrittenDeclaredMessageItemEntity
    {
        public int Id { get; set; }

        public OmniSignatureEntity? Signature { get; set; }

        public DateTime CreationTime { get; set; }

        public static WrittenDeclaredMessageItemEntity Import(WrittenDeclaredMessageItem value)
        {
            return new WrittenDeclaredMessageItemEntity() { Signature = OmniSignatureEntity.Import(value.Signature), CreationTime = value.CreationTime };
        }

        public WrittenDeclaredMessageItem Export()
        {
            return new WrittenDeclaredMessageItem(this.Signature!.Export(), this.CreationTime);
        }
    }
}
