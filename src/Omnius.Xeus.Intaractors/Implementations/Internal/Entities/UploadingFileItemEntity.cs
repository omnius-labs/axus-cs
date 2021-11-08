using System;
using Omnius.Xeus.Intaractors.Internal.Models;
using Omnius.Xeus.Intaractors.Models;

namespace Omnius.Xeus.Intaractors.Internal.Entities
{
    internal record UploadingFileItemEntity
    {
        public string? FilePath { get; set; }

        public SeedEntity? Seed { get; set; }

        public DateTime CreationTime { get; set; }

        public int State { get; set; }

        public static UploadingFileItemEntity Import(UploadingFileItem value)
        {
            return new UploadingFileItemEntity()
            {
                Seed = SeedEntity.Import(value.Seed),
                FilePath = value.FilePath,
                CreationTime = value.CreationTime,
                State = (int)value.State,
            };
        }

        public UploadingFileItem Export()
        {
            return new UploadingFileItem(this.FilePath ?? string.Empty, this.Seed?.Export() ?? Intaractors.Models.Seed.Empty, this.CreationTime, (UploadingFileState)this.State);
        }
    }
}
