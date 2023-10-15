namespace Omnius.Axus.Interactors;

public record ProfileUploaderOptions
{
    public ProfileUploaderOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
