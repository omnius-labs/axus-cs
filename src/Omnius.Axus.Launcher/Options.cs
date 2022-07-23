using CommandLine;

namespace Omnius.Axus.Launcher;

public class Options
{
    [Option('m', "mode")]
    public string? Mode { get; set; } = null;
}
