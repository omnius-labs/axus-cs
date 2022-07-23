namespace Omnius.Axus.Engines;

public record SubscribedShoutStorageOptions
{
    public SubscribedShoutStorageOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
