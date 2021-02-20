using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Omnius.Xeus.Ui.Desktop.Resources.Models
{
    public sealed class Config
    {
        public string? DaemonAddress { get; init; }

        public static async ValueTask<Config?> LoadAsync(string configPath)
        {
            try
            {
                using var stream = new FileStream(configPath, FileMode.Open);
                var serializeOptions = new JsonSerializerOptions();
                return await JsonSerializer.DeserializeAsync<Config>(stream, serializeOptions);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
