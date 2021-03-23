using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Omnius.Xeus.Utils
{
    public static class JsonHelper
    {
        public static async ValueTask<T?> ReadFileAsync<T>(string path)
        {
            using var stream = new FileStream(path, FileMode.Open);
            return await ReadStreamAsync<T>(stream);
        }

        public static async ValueTask<T?> ReadStreamAsync<T>(Stream stream)
        {
            var serializeOptions = new JsonSerializerOptions();
            return await JsonSerializer.DeserializeAsync<T>(stream, serializeOptions);
        }

        public static async ValueTask WriteFileAsync<T>(string path, T value)
        {
            using var stream = new FileStream(path, FileMode.Create);
            await WriteStreamAsync<T>(stream, value);
        }

        public static async ValueTask WriteStreamAsync<T>(Stream stream, T value, bool writeIndented = false)
        {
            var serializeOptions = new JsonSerializerOptions() { WriteIndented = writeIndented };
            await JsonSerializer.SerializeAsync(stream, value, serializeOptions);
        }
    }
}
