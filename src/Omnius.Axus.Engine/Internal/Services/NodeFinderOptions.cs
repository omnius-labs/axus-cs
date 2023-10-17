namespace Omnius.Axus.Core.Engine.Services;

public record NodeFinderOptions
{
    public required string ConfigDirectoryPath { get; init; }
    public required uint MaxSessionCount { get; init; }
}
