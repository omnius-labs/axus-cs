namespace Omnius.Axus.Engine.Internal.Services;

public record NodeFinderOptions
{
    public required string ConfigDirectoryPath { get; init; }
    public required uint MaxSessionCount { get; init; }
}
