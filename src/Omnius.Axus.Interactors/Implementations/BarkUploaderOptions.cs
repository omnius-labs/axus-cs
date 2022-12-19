namespace Omnius.Axus.Interactors;

public record BarkUploaderOptions
{
    public BarkUploaderOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
