namespace Omnius.Xeus.Intaractors
{
    public record ProfilePublisherOptions
    {
        public ProfilePublisherOptions(string configDirectoryPath)
        {
            this.ConfigDirectoryPath = configDirectoryPath;
        }

        public string ConfigDirectoryPath { get; }
    }
}
