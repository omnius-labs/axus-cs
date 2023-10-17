namespace Omnius.Axus.Interactors;

public record SeedDownloaderOptions
{
    public SeedDownloaderOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
