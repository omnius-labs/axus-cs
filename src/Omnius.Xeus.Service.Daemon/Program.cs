using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cocona;
using Omnius.Core.Helpers;

namespace Omnius.Xeus.Service.Daemon;

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

    public async ValueTask RunAsync([Option("config")] string configPath, [Option("storage")] string storageDirectoryPath, [Option("logs")] string logsDirectoryPath, [Option("verbose", new[] { 'v' })] bool verbose = false)
    {
        DirectoryHelper.CreateDirectory(storageDirectoryPath);
        DirectoryHelper.CreateDirectory(logsDirectoryPath);

        SetLogsDirectory(logsDirectoryPath);

        if (verbose)
        {
            ChangeLogLevel(NLog.LogLevel.Trace);
        }

        try
        {
            _logger.Info("Starting...");
            await Runner.EventLoopAsync(configPath, storageDirectoryPath, this.Context.CancellationToken);
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