using System;
using System.IO;

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
    }
}
