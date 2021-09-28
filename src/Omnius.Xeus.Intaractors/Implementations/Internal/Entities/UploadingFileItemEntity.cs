using System;
using LiteDB;

namespace Omnius.Xeus.Intaractors.Internal.Entities
{
    internal record UploadingFileItemEntity
    {
        [BsonCtor]
        public UploadingFileItemEntity(string filePath, SeedEntity seed, DateTime creationTime)
        {
            this.FilePath = filePath;
            this.Seed = seed;
            this.CreationTime = creationTime;
        }

        [BsonId]
        public string FilePath { get; }

        public SeedEntity Seed { get; }

        public DateTime CreationTime { get; }
    }
}
