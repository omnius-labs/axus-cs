using System.IO;

namespace Omnius.Xeus.Engine.Storages.Primitives
{
    internal static class UnitTestEnvironment
    {
        public static string TempDirectoryPath => Path.GetFullPath("Temp");
    }
}
