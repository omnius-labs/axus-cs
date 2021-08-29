namespace Omnius.Xeus.Intaractors
{
    public record UserProfileUploaderOptions
    {
        public UserProfileUploaderOptions(string configDirectoryPath)
        {
            this.ConfigDirectoryPath = configDirectoryPath;
        }

        public string ConfigDirectoryPath { get; }
    }
}
