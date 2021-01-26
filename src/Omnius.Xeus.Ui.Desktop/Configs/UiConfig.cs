using System.IO;
using System.Net;
using Omnius.Core.Network;
using Omnius.Xeus.Ui.Console.Helpers;

namespace Omnius.Xeus.Ui.Desktop.Configs
{
    public sealed class UiConfig
    {
        public string? DaemonAddress { get; init; }

        private const string ConfigFileName = "ui-desktop.yaml";

        public static UiConfig LoadConfig(string configDirectoryPath)
        {
            InitConfig(configDirectoryPath);

            var configPath = Path.Combine(configDirectoryPath, ConfigFileName);
            var config = YamlHelper.ReadFile<UiConfig>(configPath);
            return config;
        }

        private static void InitConfig(string configDirectoryPath)
        {
            if (!Directory.Exists(configDirectoryPath))
            {
                Directory.CreateDirectory(configDirectoryPath);
            }

            var config = CreateInitConfig();

            YamlHelper.WriteFile(Path.Combine(configDirectoryPath, ConfigFileName), config);
        }

        private static UiConfig CreateInitConfig()
        {
            return new UiConfig()
            {
                DaemonAddress = (string?)OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 32321),
            };
        }
    }
}
