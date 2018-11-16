using CommandLine;

namespace Amoeba.Daemon
{
    class AmoebaDaemonOptions
    {
        [Option('c', "config")]
        public string ConfigFilePath { get; set; }
    }
}
