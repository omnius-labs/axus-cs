using Omnius.Core.Avalonia.Models;
using Omnius.Core.Helpers;
using Omnius.Core.Utils;

namespace Omnius.Xeus.Ui.Desktop.Configuration;

public sealed partial class UiStatus
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    public int Version { get; init; }

    public MainWindowStatus? MainWindow { get; set; }

    public SettingsWindowStatus? SettingsWindow { get; set; }

    public TextWindowStatus? TextWindow { get; set; }

    public DownloadControlStatus? DownloadControl { get; set; }

    public static async ValueTask<UiStatus> LoadAsync(string configPath)
    {
        UiStatus? result = null;

        try
        {
            result = await JsonHelper.ReadFileAsync<UiStatus>(configPath);
        }
        catch (Exception e)
        {
            _logger.Debug(e);
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
    public WindowStatus? Window { get; set; }
}

public sealed class SettingsWindowStatus
{
    public WindowStatus? Window { get; set; }
}

public sealed class TextWindowStatus
{
    public WindowStatus? Window { get; set; }
}

public sealed class DownloadControlStatus
{
    public double DataGrid_Rate_Width { get; set; }
}
