using System.Reflection;
using Cocona;
using Omnius.Core.Helpers;
using Omnius.Core.Net;

namespace Omnius.Axis.Daemon;

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
        if (e.ExceptionObject is Exception ex)
        {
            _logger.Error(ex, "Unhandled Exception");
        }
    }

    public async ValueTask RunAsync([Option("storage", new char[] { 's' })] string storageDirectoryPath, [Option("listen", new char[] { 'l' })] string listenAddress, [Option("verbose", new char[] { 'v' })] bool verbose = false)
    {
        FileStream? lockFileStream = null;

        try
        {
            DirectoryHelper.CreateDirectory(storageDirectoryPath);

            SetLogsDirectory(Path.Combine(storageDirectoryPath, "logs"));
            if (verbose) ChangeLogLevel(NLog.LogLevel.Trace);

            lockFileStream = new FileStream(Path.Combine(storageDirectoryPath, "lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose);

            _logger.Info("Starting...");
            _logger.Info("AssemblyInformationalVersion: {0}", Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);

            await Runner.EventLoopAsync(Path.Combine(storageDirectoryPath, "db"), OmniAddress.Parse(listenAddress), this.Context.CancellationToken);
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e, "Operation Canceled");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }
        finally
        {
            _logger.Info("Stopping...");
            NLog.LogManager.Shutdown();

            lockFileStream?.Dispose();
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
