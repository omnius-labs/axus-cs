namespace Omnius.Xeus.Intaractors
{
    public record UserProfileFinderOptions
    {
        public UserProfileFinderOptions(string configDirectoryPath)
        {
            this.ConfigDirectoryPath = configDirectoryPath;
        }

        public string ConfigDirectoryPath { get; }
    }
}
