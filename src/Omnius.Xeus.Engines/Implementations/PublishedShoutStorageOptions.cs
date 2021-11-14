namespace Omnius.Xeus.Engines;

public record PublishedShoutStorageOptions
{
    public PublishedShoutStorageOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
