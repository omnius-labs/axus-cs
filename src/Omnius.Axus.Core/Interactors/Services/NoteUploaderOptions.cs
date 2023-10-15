namespace Omnius.Axus.Interactors;

public record NoteUploaderOptions
{
    public NoteUploaderOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
