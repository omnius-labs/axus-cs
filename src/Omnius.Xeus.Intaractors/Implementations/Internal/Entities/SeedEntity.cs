using System;
using LiteDB;

namespace Omnius.Xeus.Intaractors.Internal.Entities
{
    internal record SeedEntity
    {
        [BsonCtor]
        public SeedEntity(OmniHashEntity rootHash, string name, DateTime creationTime, ulong size)
        {
            this.RootHash = rootHash;
            this.Name = name;
            this.CreationTime = creationTime;
            this.Size = size;
        }

        public OmniHashEntity RootHash { get; }
        public string Name { get; }
        public DateTime CreationTime { get; }
        public ulong Size { get; }
    }
}
