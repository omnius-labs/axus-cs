namespace Omnius.Axis.Intaractors;

public record ProfileSubscriberOptions
{
    public ProfileSubscriberOptions(string configDirectoryPath)
    {
        this.ConfigDirectoryPath = configDirectoryPath;
    }

    public string ConfigDirectoryPath { get; }
}
