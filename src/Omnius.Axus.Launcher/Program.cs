namespace Omnius.Axus.Launcher;

public class Program
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    public static async Task Main(string[] args)
    {
        if (Updater.TryUpdate()) return;

        await Runner.RunAsync();
    }
}
