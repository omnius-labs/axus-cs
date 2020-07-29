using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Omnius.Xeus.Service.Internal
{
    public static class TestHelpers
    {
        private static readonly Random _random = new Random();

        public static string GetBasePath()
        {
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly()!.Location)!;
            return basePath;
        }

        public static IDisposable GenTempDirectory(out string path)
        {
            var result = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(result);
            path = result;
            return new DirectoryDeleter(path);
        }

        private sealed class DirectoryDeleter : IDisposable
        {
            public DirectoryDeleter(string path) => this.Path = path;
            public void Dispose() => Directory.Delete(this.Path, true);
            private string Path { get; }
        }

        public static byte[] GetRandomBytes(int length)
        {
            var result = new byte[length];
            _random.NextBytes(result);
            return result;
        }

        // https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings
        private static readonly char[] _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        public static string GetRandomString(int size)
        {
            byte[] data = new byte[4 * size];
            using var crypto = new RNGCryptoServiceProvider();
            crypto.GetBytes(data);

            var result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % _chars.Length;

                result.Append(_chars[idx]);
            }

            return result.ToString();
        }

        // https://stackoverflow.com/questions/14505932/random-datetime-between-range-not-unified-output/14511053
        public static DateTime GetRandomDateTimeUtc(DateTime from, DateTime to)
        {
            var range = to - from;
            var randTimeSpan = new TimeSpan((long)(_random.NextDouble() * range.Ticks));
            return (from + randTimeSpan).ToUniversalTime();
        }
    }
}
