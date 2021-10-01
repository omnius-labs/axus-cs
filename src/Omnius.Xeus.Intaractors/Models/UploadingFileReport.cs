using System;

namespace Omnius.Xeus.Intaractors.Models
{
    public record UploadingFileReport
    {
        public UploadingFileReport(string filePath, Seed? seed, DateTime creationTime, UploadingFileState state)
        {
            this.FilePath = filePath;
            this.Seed = seed;
            this.CreationTime = creationTime;
            this.State = state;
        }

        public string FilePath { get; }
        public Seed? Seed { get; }
        public DateTime CreationTime { get; }
        public UploadingFileState State { get; }
    }
}
