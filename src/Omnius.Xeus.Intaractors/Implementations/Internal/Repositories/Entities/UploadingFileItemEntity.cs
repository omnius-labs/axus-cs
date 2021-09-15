using System;
using LiteDB;
using Omnius.Core.RocketPack;
using Omnius.Xeus.Intaractors.Internal.Models;

namespace Omnius.Xeus.Intaractors.Internal.Repositories.Entities
{
    internal record UploadingFileItemEntity
    {
        [BsonId]
        public string? FilePath { get; set; }

        public DateTime CreationTime { get; set; }

        public static UploadingFileItemEntity Import(UploadingFileItem value)
        {
            return new UploadingFileItemEntity()
            {
                FilePath = value.FilePath,
                CreationTime = value.CreationTime.ToDateTime(),
            };
        }

        public UploadingFileItem Export()
        {
            return new UploadingFileItem(this.FilePath ?? string.Empty, Timestamp.FromDateTime(this.CreationTime));
        }
    }
}
