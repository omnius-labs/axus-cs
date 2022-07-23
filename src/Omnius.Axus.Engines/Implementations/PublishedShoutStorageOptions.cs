namespace Omnius.Axus.Engines;

public record PublishedShoutStorageOptions
{
    public PublishedShoutStorageOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
