namespace Omnius.Xeus.Services
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
