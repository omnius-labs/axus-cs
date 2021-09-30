using System;

namespace Omnius.Xeus.Intaractors.Models
{
    public record UploadingFileReport
    {
        public UploadingFileReport(string filePath, DateTime creationTime, Seed? seed, UploadingFileState state)
        {
            this.FilePath = filePath;
            this.CreationTime = creationTime;
            this.Seed = seed;
            this.State = state;
        }

        public string FilePath { get; }
        public DateTime CreationTime { get; }
        public Seed? Seed { get; }
        public UploadingFileState State { get; }
    }
}
