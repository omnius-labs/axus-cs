namespace Omnius.Axus.Engines;

public record NodeFinderOptions
{
    public required string ConfigDirectoryPath { get; init; }
    public required uint MaxSessionCount { get; init; }
}
