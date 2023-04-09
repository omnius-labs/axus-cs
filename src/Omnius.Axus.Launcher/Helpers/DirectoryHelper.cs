
namespace Omnius.Axus.Launcher.Helpers;

public static class DirectoryHelper
{
    public static void CreateOrTruncate(string path)
    {
        if (Directory.Exists(path)) Directory.Delete(path, true);
        Directory.CreateDirectory(path);
    }
}
