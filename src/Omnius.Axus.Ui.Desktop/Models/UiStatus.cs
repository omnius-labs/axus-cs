using Omnius.Core.Helpers;
using Omnius.Core.Utils;

namespace Omnius.Axus.Ui.Desktop.Models;

public sealed class UiStatus
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    public int Version { get; set; }

    public MainWindowStatus? MainWindow { get; set; }

    public SettingsWindowStatus? SettingsWindow { get; set; }

    public TextEditWindowStatus? TextEditWindow { get; set; }

    public DownloadViewStatus? DownloadView { get; set; }

    public static async ValueTask<UiStatus> LoadAsync(string configPath)
    {
        UiStatus? result = null;

        try
        {
            result = await JsonHelper.ReadFileAsync<UiStatus>(configPath);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }

        result ??= new UiStatus();

        return result;
    }

    public async ValueTask SaveAsync(string configPath)
    {
        DirectoryHelper.CreateDirectory(Path.GetDirectoryName(configPath)!);
        await JsonHelper.WriteFileAsync(configPath, this, true);
    }
}

public sealed class MainWindowStatus
{
}

public sealed class SettingsWindowStatus
{
}

public sealed class TextEditWindowStatus
{
}

public sealed class DownloadViewStatus
{
    public double DataGrid_Rate_Width { get; set; }
}
