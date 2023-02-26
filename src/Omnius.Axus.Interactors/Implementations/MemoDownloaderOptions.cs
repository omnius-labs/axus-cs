namespace Omnius.Axus.Interactors;

public record MemoDownloaderOptions
{
    public MemoDownloaderOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
