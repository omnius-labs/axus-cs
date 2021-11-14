namespace Omnius.Xeus.Service.Engines;

public record SubscribedShoutStorageOptions
{
    public SubscribedShoutStorageOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
