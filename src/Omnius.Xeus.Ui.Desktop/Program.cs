using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using CommandLine;
using Omnius.Core.Helpers;

namespace Omnius.Xeus.Ui.Desktop
{
    public class Program
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program.UnhandledException);

            CommandLine.Parser.Default.ParseArguments<Options>(args)
              .WithParsed(Run)
              .WithNotParsed(HandleParseError);
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Error(e);
        }

        private static void Run(Options o)
        {
            DirectoryHelper.CreateDirectory(o.StorageDirectoryPath);
            DirectoryHelper.CreateDirectory(o.LogsDirectoryPath);

            SetLogsDirectory(o.LogsDirectoryPath);

            if (o.Verbose)
            {
                ChangeLogLevel(NLog.LogLevel.Trace);
            }

            Bootstrapper.Instance.Build(o.ConfigPath, o.StorageDirectoryPath);

            _logger.Info("Starting...");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(Environment.GetCommandLineArgs(), ShutdownMode.OnMainWindowClose);
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

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (var err in errs)
            {
                _logger.Error(err);
            }
        }
    }
}
