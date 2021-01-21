using System.IO;
using System.Text;
using YamlDotNet;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Omnius.Xeus.Ui.Console.Helpers
{
    public static class YamlHelper
    {
        public static T ReadFile<T>(string path)
        {
            using var stream = new FileStream(path, FileMode.Open);
            return ReadStream<T>(stream);
        }

        public static T ReadStream<T>(Stream stream)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();
            using var reader = new StreamReader(stream);
            return deserializer.Deserialize<T>(reader);
        }

        public static void WriteFile(string path, object value)
        {
            using var stream = new FileStream(path, FileMode.Create);
            WriteStream(stream, value);
        }

        public static void WriteStream(Stream stream, object value)
        {
            var serializer = new SerializerBuilder()
                .Build();
            using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            serializer.Serialize(writer, value);
        }
    }
}
