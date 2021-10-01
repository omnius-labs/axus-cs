using System;

namespace Omnius.Xeus.Intaractors.Models
{
    public record DownloadingFileReport
    {
        public DownloadingFileReport(Seed seed, string? filePath, DateTime creationTime, DownloadingFileState state)
        {
            this.Seed = seed;
            this.FilePath = filePath;
            this.CreationTime = creationTime;
            this.State = state;
        }

        public Seed Seed { get; }
        public string? FilePath { get; }
        public DateTime CreationTime { get; }
        public DownloadingFileState State { get; }
    }
}
