using Avalonia;
using Avalonia.Controls;
using CommandLine;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Core.Helpers;
using Omnius.Core.Net;

namespace Omnius.Axis.Ui.Desktop;

public static class Program
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private static async Task Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program.UnhandledException);

        var parsedResult = CommandLine.Parser.Default.ParseArguments<Options>(args);
        parsedResult = await parsedResult.WithParsedAsync(RunAsync);
        parsedResult.WithNotParsed(HandleParseError);
    }

    private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        _logger.Error(e);
    }

    private static async Task RunAsync(Options options)
    {
        await InitAsync(options);

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(Environment.GetCommandLineArgs(), ShutdownMode.OnMainWindowClose);

        await DisposeAsync();
    }

    private static async ValueTask InitAsync(Options options)
    {
        if (options.IsDesignMode)
        {
            Program.IsDesignMode = true;
            return;
        }

        DirectoryHelper.CreateDirectory(options.StorageDirectoryPath!);

        var databaseDirectoryPath = Path.Combine(options.StorageDirectoryPath!, "db");
        var logsDirectoryPath = Path.Combine(options.StorageDirectoryPath!, "logs");

        DirectoryHelper.CreateDirectory(databaseDirectoryPath!);
        DirectoryHelper.CreateDirectory(logsDirectoryPath!);

        SetLogsDirectory(logsDirectoryPath);

        if (options.Verbose) ChangeLogLevel(NLog.LogLevel.Trace);

        _logger.Info("Starting...");
        Bootstrapper.Instance.Build(databaseDirectoryPath, OmniAddress.Parse(options.ListenAddress!));
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

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();

    private static async ValueTask DisposeAsync()
    {
        await Bootstrapper.Instance.DisposeAsync();

        _logger.Info("Stopping...");
        NLog.LogManager.Shutdown();
    }

    private static void HandleParseError(IEnumerable<Error> errs)
    {
        foreach (var err in errs)
        {
            _logger.Error(err);
        }
    }

    private static bool _isDesignMode;

    public static bool IsDesignMode
    {
        get => (Design.IsDesignMode || _isDesignMode);
        set => _isDesignMode = value;
    }
}
