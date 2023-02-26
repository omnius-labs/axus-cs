namespace Omnius.Axus.Interactors;

public record MemoUploaderOptions
{
    public MemoUploaderOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
