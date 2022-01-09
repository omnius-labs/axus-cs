using CommandLine;

namespace Omnius.Axis.Ui.Desktop;

public class Options
{
    [Option('l', "listen")]
    public string? ListenAddress { get; set; }

    [Option('s', "storage")]
    public string? StorageDirectoryPath { get; set; }

    [Option('v', "verbose")]
    public bool Verbose { get; set; } = false;
}
