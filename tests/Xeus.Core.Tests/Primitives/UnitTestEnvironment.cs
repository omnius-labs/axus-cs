using System.IO;

namespace Xeus.Core.Tests.Primitives
{
    internal static class UnitTestEnvironment
    {
        public static string TempDirectoryPath => Path.GetFullPath("Temp");
    }
}
