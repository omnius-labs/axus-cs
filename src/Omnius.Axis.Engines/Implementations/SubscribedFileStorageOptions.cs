namespace Omnius.Axis.Engines;

public record SubscribedFileStorageOptions
{
    public SubscribedFileStorageOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
