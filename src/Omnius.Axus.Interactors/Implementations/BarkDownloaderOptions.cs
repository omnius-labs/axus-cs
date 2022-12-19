namespace Omnius.Axus.Interactors;

public record BarkDownloaderOptions
{
    public BarkDownloaderOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
