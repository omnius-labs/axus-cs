using System;
using System.IO;

namespace Omnius.Xeus.Engines.Internal.Helpers
{
    internal static class DirectoryHelper
    {
        public static void CreateDirectory(string? path)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
