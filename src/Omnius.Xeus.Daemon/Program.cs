using Cocona;
using Omnius.Core.Helpers;
using Omnius.Xeus.Daemon.Configuration;

namespace Omnius.Xeus.Daemon;

public class Program : CoconaLiteConsoleAppBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program.UnhandledException);
        CoconaLiteApp.Run<Program>(args);
    }

    private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        _logger.Error(e);
    }

    public async ValueTask RunAsync([Option("config")] string configPath)
    {
        var appConfig = await AppConfig.LoadAsync(configPath);

        DirectoryHelper.CreateDirectory(appConfig.StorageDirectoryPath!);
        DirectoryHelper.CreateDirectory(appConfig.LogsDirectoryPath!);

        SetLogsDirectory(appConfig.LogsDirectoryPath!);

        if (appConfig.Verbose) ChangeLogLevel(NLog.LogLevel.Trace);

        try
        {
            _logger.Info("Starting...");
            await Runner.EventLoopAsync(appConfig, this.Context.CancellationToken);
        }
        finally
        {
            _logger.Info("Stopping...");
            NLog.LogManager.Shutdown();
        }
    }

    private static void SetLogsDirectory(string logsDirectoryPath)
    {
        var target = (NLog.Targets.FileTarget)NLog.LogManager.Configuration.FindTargetByName("log_file");
        target.FileName = $"{Path.GetFullPath(logsDirectoryPath)}/${{date:format=yyyy-MM-dd}}.log";
        target.ArchiveFileName = $"{Path.GetFullPath(logsDirectoryPath)}/logs/archive.{{#}}.log";
        NLog.LogManager.ReconfigExistingLoggers();
    }

    private static void ChangeLogLevel(NLog.LogLevel minLevel)
    {
        _logger.Debug("Log level changed: {0}", minLevel);

        var rootLoggingRule = NLog.LogManager.Configuration.LoggingRules.First(n => n.NameMatches("*"));
        rootLoggingRule.EnableLoggingForLevels(minLevel, NLog.LogLevel.Fatal);
        NLog.LogManager.ReconfigExistingLoggers();
    }
}
