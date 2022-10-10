using Omnius.Core.Net;

namespace Omnius.Axus.Ui.Desktop.Models;

public record class AxusEnvironment
{
    public string StorageDirectoryPath { get; init; } = string.Empty;
    public string DatabaseDirectoryPath { get; init; } = string.Empty;
    public string LogsDirectoryPath { get; init; } = string.Empty;
    public OmniAddress ListenAddress { get; init; } = OmniAddress.Empty;
}
