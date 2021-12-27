namespace Omnius.Axis.Ui.Desktop;

public class Options
{
    [CommandLine.Option('l', "listen", Required = true)]
    public string ListenAddress { get; set; } = null!;

    [CommandLine.Option('s', "storage", Required = true)]
    public string StorageDirectoryPath { get; set; } = null!;

    [CommandLine.Option('v', "verbose")]
    public bool Verbose { get; set; } = false;
}
