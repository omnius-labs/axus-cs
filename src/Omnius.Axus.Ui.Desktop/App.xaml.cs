using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Axus.Ui.Desktop.Internal;
using Omnius.Axus.Ui.Desktop.Models;
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
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifeTime)
            {
                var mainWindow = new MainWindow();
                mainWindow.ViewModel = new MainWindowDesignModel();
                lifeTime.MainWindow = mainWindow;
            }

            return;
        }

        var parsedResult = CommandLine.Parser.Default.ParseArguments<Options>(Environment.GetCommandLineArgs());
        parsedResult.WithParsed(this.OnParsed);
    }

    private async void OnParsed(Options options)
    {
        try
        {
            DirectoryHelper.CreateDirectory(options.StorageDirectoryPath);

            var axusEnvironment = new AxusEnvironment()
            {
                StorageDirectoryPath = options.StorageDirectoryPath,
                DatabaseDirectoryPath = Path.Combine(options.StorageDirectoryPath, "db"),
                LogsDirectoryPath = Path.Combine(options.StorageDirectoryPath, "logs"),
                ListenAddress = OmniAddress.Parse(options.ListenAddress),
            };

            DirectoryHelper.CreateDirectory(axusEnvironment.DatabaseDirectoryPath);
            DirectoryHelper.CreateDirectory(axusEnvironment.LogsDirectoryPath);

            SetLogsDirectory(axusEnvironment.LogsDirectoryPath);

            if (options.Verbose) ChangeLogLevel(NLog.LogLevel.Trace);

            _lockFileStream = new FileStream(Path.Combine(options.StorageDirectoryPath, "lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose);

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
        target.ArchiveFileName = $"{Path.GetFullPath(logsDirectoryPath)}/logs/archive.{{#}}.log";
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
