using System;
using LiteDB;

namespace Omnius.Xeus.Intaractors.Internal.Entities
{
    internal record DownloadingFileItemEntity
    {
        [BsonCtor]
        public DownloadingFileItemEntity(SeedEntity seed, DateTime creationTime)
        {
            this.Seed = seed;
            this.CreationTime = creationTime;
        }

        public SeedEntity Seed { get; }

        public DateTime CreationTime { get; }
    }
}
