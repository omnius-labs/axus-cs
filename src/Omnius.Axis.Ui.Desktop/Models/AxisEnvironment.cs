using Omnius.Core.Net;

namespace Omnius.Axis.Ui.Desktop.Models;

public sealed class AxisEnvironment
{
    public AxisEnvironment(string storageDirectoryPath, string databaseDirectoryPath, string logsDirectoryPath, OmniAddress listenAddress)
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
