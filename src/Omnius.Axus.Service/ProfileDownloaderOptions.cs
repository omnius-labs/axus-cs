namespace Omnius.Axus.Interactors;

public record ProfileDownloaderOptions
{
    public ProfileDownloaderOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
