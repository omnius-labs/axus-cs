namespace Omnius.Xeus.Intaractors
{
    public record UserProfileDownloaderOptions
    {
        public UserProfileDownloaderOptions(string configDirectoryPath)
        {
            this.ConfigDirectoryPath = configDirectoryPath;
        }

        public string ConfigDirectoryPath { get; }
    }
}
