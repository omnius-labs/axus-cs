namespace Omnius.Xeus.Ui.Desktop
{
    public class Options
    {
        [CommandLine.Option("config", Required = true)]
        public string ConfigPath { get; set; } = string.Empty;

        [CommandLine.Option("storage", Required = true)]
        public string StorageDirectoryPath { get; set; } = string.Empty;

        [CommandLine.Option("logs", Required = true)]
        public string LogsDirectoryPath { get; set; } = string.Empty;

        [CommandLine.Option('v', "verbose", Default = false)]
        public bool Verbose { get; set; }
    }
}
