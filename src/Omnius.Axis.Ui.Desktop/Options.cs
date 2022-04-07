using CommandLine;
using Omnius.Core.Net;

namespace Omnius.Axis.Ui.Desktop;

public class Options
{
    [Option('l', "listen")]
    public string ListenAddress { get; set; } = OmniAddress.Empty.ToString();

    [Option('s', "storage")]
    public string StorageDirectoryPath { get; set; } = "../storage";

    [Option('v', "verbose")]
    public bool Verbose { get; set; } = false;
}
