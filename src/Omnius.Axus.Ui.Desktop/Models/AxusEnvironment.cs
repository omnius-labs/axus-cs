using Omnius.Core.Net;

namespace Omnius.Axus.Ui.Desktop.Models;

public sealed class AxusEnvironment
{
    public AxusEnvironment(string storageDirectoryPath, string databaseDirectoryPath, string logsDirectoryPath, OmniAddress listenAddress)
    {
        this.StorageDirectoryPath = storageDirectoryPath;
        this.DatabaseDirectoryPath = databaseDirectoryPath;
        this.LogsDirectoryPath = logsDirectoryPath;
        this.ListenAddress = listenAddress;
    }

    public string StorageDirectoryPath { get; }

    public string DatabaseDirectoryPath { get; }

    public string LogsDirectoryPath { get; }

    public OmniAddress ListenAddress { get; }
}
