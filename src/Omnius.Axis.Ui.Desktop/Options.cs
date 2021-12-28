namespace Omnius.Axis.Ui.Desktop;

public class Options
{
    [CommandLine.Option('l', "listen")]
    public string? ListenAddress { get; set; }

    [CommandLine.Option('s', "storage")]
    public string? StorageDirectoryPath { get; set; }

    [CommandLine.Option('v', "verbose")]
    public bool Verbose { get; set; } = false;

    [CommandLine.Option('d', "design")]
    public bool IsDesignMode { get; set; } = false;
}
