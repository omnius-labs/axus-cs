namespace Omnius.Axus.Interactors;

public record FileUploaderOptions
{
    public FileUploaderOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
