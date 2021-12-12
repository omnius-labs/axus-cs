namespace Omnius.Axis.Engines;

public record PublishedFileStorageOptions
{
    public PublishedFileStorageOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
