using Omnius.Core.Net;

namespace Omnius.Axus.Ui.Desktop.Configuration;

public record class AxusEnvironment
{
    public required string StorageDirectoryPath { get; init; }
    public required string DatabaseDirectoryPath { get; init; }
    public required string LogsDirectoryPath { get; init; }
    public required OmniAddress ListenAddress { get; init; }
}
