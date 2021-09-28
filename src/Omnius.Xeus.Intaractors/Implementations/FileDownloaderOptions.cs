namespace Omnius.Xeus.Intaractors
{
    public record FileDownloaderOptions
    {
        public FileDownloaderOptions(string configDirectoryPath)
        {
            this.ConfigDirectoryPath = configDirectoryPath;
        }

        public string ConfigDirectoryPath { get; }
    }
}
