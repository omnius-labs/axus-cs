namespace Omnius.Axus.Interactors;

public record BarkPublisherOptions
{
    public BarkPublisherOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
