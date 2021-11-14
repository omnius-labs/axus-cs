namespace Omnius.Xeus.Engines;

public record PublishedFileStorageOptions
{
    public PublishedFileStorageOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
