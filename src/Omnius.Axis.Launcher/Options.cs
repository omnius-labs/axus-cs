using CommandLine;

namespace Omnius.Axis.Launcher;

public class Options
{
    [Option('m', "mode")]
    public string? Mode { get; set; } = null;
}
