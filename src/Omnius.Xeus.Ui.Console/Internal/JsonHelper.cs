using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using YamlDotNet;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Omnius.Xeus.Ui.Console.Internal
{
    public static class JsonHelper
    {
        public static void WriteStream<T>(Stream stream, T value)
        {
            var options = new JsonWriterOptions()
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            using var utf8JsonWriter = new Utf8JsonWriter(stream, options);
            JsonSerializer.Serialize<T>(utf8JsonWriter, value);
        }
    }
}
