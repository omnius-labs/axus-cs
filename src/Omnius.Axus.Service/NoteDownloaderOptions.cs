namespace Omnius.Axus.Interactors;

public record NoteDownloaderOptions
{
    public NoteDownloaderOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
