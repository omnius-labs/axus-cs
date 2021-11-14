using Omnius.Core.Helpers;
using Omnius.Core.Utils;

namespace Omnius.Xeus.Ui.Desktop.Configuration;

public sealed class AppSettings
{
    public List<string> TrustedSignatures { get; init; } = new List<string>();

    public List<string> BlockedSignatures { get; init; } = new List<string>();

    public int SearchProfileDepth { get; }

    public static async ValueTask<AppSettings?> LoadAsync(string configPath)
    {
        try
        {
            return await JsonHelper.ReadFileAsync<AppSettings>(configPath);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async ValueTask SaveAsync(string configPath)
    {
        DirectoryHelper.CreateDirectory(Path.GetDirectoryName(configPath)!);
        await JsonHelper.WriteFileAsync(configPath, this, true);
    }
}
