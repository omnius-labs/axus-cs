using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Omnius.Xeus.Ui.Desktop.Internal;

namespace Omnius.Xeus.Ui.Desktop.Resources.Models
{
    public sealed class Config
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public int Version { get; init; }

        public string? DaemonAddress { get; init; }

        public static async ValueTask<Config?> LoadAsync(string configPath)
        {
            try
            {
                return YamlHelper.ReadFile<Config>(configPath);
            }
            catch (Exception e)
            {
                _logger.Debug(e);
                return null;
            }
        }

        public async ValueTask SaveAsync(string configPath)
        {
            YamlHelper.WriteFile(configPath, this);
        }
    }
}
