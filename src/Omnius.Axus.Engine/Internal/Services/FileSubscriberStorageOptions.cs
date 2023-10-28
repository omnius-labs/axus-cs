namespace Omnius.Axus.Engine.Internal.Services;

public record FileSubscriberStorageOptions
{
    public required string ConfigDirectoryPath { get; init; }
}
