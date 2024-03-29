using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Axus.Ui.Desktop.Configuration;
using Omnius.Axus.Ui.Desktop.Internal;
using Omnius.Axus.Ui.Desktop.Windows.Main;
using Omnius.Core.Helpers;
using Omnius.Core.Net;

namespace Omnius.Axus.Ui.Desktop;

public class App : Application
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private FileStream? _lockFileStream;

    public override void Initialize()
    {
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((_, e) => _logger.Error(e));
        this.ApplicationLifetime!.Exit += (_, _) => this.Exit();

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        this.Startup();

        base.OnFrameworkInitializationCompleted();
    }

    public static new App? Current => Application.Current as App;

    public new IClassicDesktopStyleApplicationLifetime? ApplicationLifetime => base.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

    public MainWindow? MainWindow => this.ApplicationLifetime?.MainWindow as MainWindow;

    public bool IsDesignMode
    {
        get
        {
#if DESIGN
            return true;
#else
            return Design.IsDesignMode;
#endif
        }
    }

    private void Startup()
    {
        if (this.IsDesignMode)
        {
            var parsedResult = CommandLine.Parser.Default.ParseArguments<DesignModeArgs>(Environment.GetCommandLineArgs());
            parsedResult.WithParsed(this.OnDesignModeArgsParsed);
        }
        else
        {
            var parsedResult = CommandLine.Parser.Default.ParseArguments<NormalModeArgs>(Environment.GetCommandLineArgs());
            parsedResult.WithParsed(this.OnNormalModeArgsParsed);
        }
    }

    public class DesignModeArgs
    {
        [Option('d', "design")]
        public string DesignTargetName { get; set; } = "Main";
    }

    private async void OnDesignModeArgsParsed(DesignModeArgs args)
    {
        if (this.IsDesignMode)
        {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifeTime)
            {
                switch (args.DesignTargetName)
                {
                    case "Main":
                        var mainWindow = new MainWindow();
                        mainWindow.ViewModel = new MainWindowDesignModel();
                        lifeTime.MainWindow = mainWindow;
                        break;
                }
            }

            return;
        }
    }

    public class NormalModeArgs
    {
        [Option('l', "listen")]
        public string ListenAddress { get; set; } = OmniAddress.Empty.ToString();

        [Option('s', "storage")]
        public string StorageDirectoryPath { get; set; } = "../storage";

        [Option('v', "verbose")]
        public bool Verbose { get; set; } = false;
    }

    private async void OnNormalModeArgsParsed(NormalModeArgs args)
    {
        try
        {
            DirectoryHelper.CreateDirectory(args.StorageDirectoryPath);

            var axusEnvironment = new AxusEnvironment()
            {
                StorageDirectoryPath = args.StorageDirectoryPath,
                DatabaseDirectoryPath = Path.Combine(args.StorageDirectoryPath, "db"),
                LogsDirectoryPath = Path.Combine(args.StorageDirectoryPath, "logs"),
                ListenAddress = OmniAddress.Parse(args.ListenAddress),
            };

            DirectoryHelper.CreateDirectory(axusEnvironment.DatabaseDirectoryPath);
            DirectoryHelper.CreateDirectory(axusEnvironment.LogsDirectoryPath);

            SetLogsDirectory(axusEnvironment.LogsDirectoryPath);

            if (args.Verbose) ChangeLogLevel(NLog.LogLevel.Trace);

            _lockFileStream = new FileStream(Path.Combine(args.StorageDirectoryPath, "lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose);

            _logger.Info("Starting...");
            _logger.Info("AssemblyInformationalVersion: {0}", Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);

            var mainWindow = new MainWindow(Path.Combine(axusEnvironment.DatabaseDirectoryPath, "windows", "main"));

            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
                lifetime.MainWindow = mainWindow;
            }

            await Bootstrapper.Instance.BuildAsync(axusEnvironment);

            var serviceProvider = Bootstrapper.Instance.GetServiceProvider();
            mainWindow.ViewModel = serviceProvider.GetRequiredService<MainWindowModel>();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }
    }

    private void SetLogsDirectory(string logsDirectoryPath)
    {
        var target = (NLog.Targets.FileTarget)NLog.LogManager.Configuration.FindTargetByName("log_file");
        target.FileName = $"{Path.GetFullPath(logsDirectoryPath)}/${{date:format=yyyy-MM-dd}}.log";
        target.ArchiveFileName = $"{Path.GetFullPath(logsDirectoryPath)}/archives/{{#}}.log";
        NLog.LogManager.ReconfigExistingLoggers();
    }

    private void ChangeLogLevel(NLog.LogLevel minLevel)
    {
        _logger.Debug("Log level changed: {0}", minLevel);

        var rootLoggingRule = NLog.LogManager.Configuration.LoggingRules.First(n => n.NameMatches("*"));
        rootLoggingRule.EnableLoggingForLevels(minLevel, NLog.LogLevel.Fatal);
        NLog.LogManager.ReconfigExistingLoggers();
    }

    private async void Exit()
    {
        await Bootstrapper.Instance.DisposeAsync();

        _logger.Info("Stopping...");
        NLog.LogManager.Shutdown();

        _lockFileStream?.Dispose();
    }
}
