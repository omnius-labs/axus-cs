using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Omnius.Core.Helpers;

namespace Omnius.Lxna.Ui.Desktop.Resources.Models
{
    public sealed partial class UiSettings
    {
        public static async ValueTask<UiSettings?> LoadAsync(string configPath)
        {
            try
            {
                using var stream = new FileStream(configPath, FileMode.Open);
                var serializeOptions = new JsonSerializerOptions();
                return await JsonSerializer.DeserializeAsync<UiSettings>(stream, serializeOptions);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async ValueTask SaveAsync(string configPath)
        {
            DirectoryHelper.CreateDirectory(Path.GetDirectoryName(configPath)!);

            using var stream = new FileStream(configPath, FileMode.Create);
            var serializeOptions = new JsonSerializerOptions() { WriteIndented = true };
            await JsonSerializer.SerializeAsync(stream, this, serializeOptions);
        }
    }
}
