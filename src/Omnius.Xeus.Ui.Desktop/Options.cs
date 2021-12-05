namespace Omnius.Xeus.Ui.Desktop;

public class Options
{
    [CommandLine.Option("config", Required = true)]
    public string ConfigPath { get; set; } = string.Empty;
}
