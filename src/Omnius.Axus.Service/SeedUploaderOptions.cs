namespace Omnius.Axus.Interactors;

public record SeedUploaderOptions
{
    public SeedUploaderOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
