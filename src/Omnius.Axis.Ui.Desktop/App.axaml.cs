using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Axis.Ui.Desktop.Views.Main;
using Omnius.Core.Helpers;
using Omnius.Core.Net;

namespace Omnius.Axis.Ui.Desktop;

public class App : Application
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    public static new App Current => (App)Application.Current;

    public override void Initialize()
    {
        if (!this.IsDesignMode)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((_, e) => _logger.Error(e));

            this.ApplicationLifetime.Startup += (_, _) => this.Startup();
            this.ApplicationLifetime.Exit += (_, _) => this.Exit();
        }

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (base.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public new IClassicDesktopStyleApplicationLifetime ApplicationLifetime => (IClassicDesktopStyleApplicationLifetime)base.ApplicationLifetime;

    public MainWindow? MainWindow
    {
        get => this.ApplicationLifetime.MainWindow as MainWindow;
        set => this.ApplicationLifetime.MainWindow = value;
    }

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

    private async void Startup()
    {
        var parsedResult = CommandLine.Parser.Default.ParseArguments<Options>(Environment.GetCommandLineArgs());
        parsedResult = await parsedResult.WithParsedAsync(this.RunAsync);
        parsedResult.WithNotParsed(this.HandleParseError);
    }

    private async Task RunAsync(Options options)
    {
        DirectoryHelper.CreateDirectory(options.StorageDirectoryPath!);

        var databaseDirectoryPath = Path.Combine(options.StorageDirectoryPath!, "db");
        var logsDirectoryPath = Path.Combine(options.StorageDirectoryPath!, "logs");

        DirectoryHelper.CreateDirectory(databaseDirectoryPath!);
        DirectoryHelper.CreateDirectory(logsDirectoryPath!);

        SetLogsDirectory(logsDirectoryPath);

        if (options.Verbose) ChangeLogLevel(NLog.LogLevel.Trace);

        _logger.Info("Starting...");
        _logger.Info("AssemblyInformationalVersion: {0}", Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);

        await Bootstrapper.Instance.BuildAsync(databaseDirectoryPath, OmniAddress.Parse(options.ListenAddress!));

        var serviceProvider = Bootstrapper.Instance.GetServiceProvider();
        var viewModel = serviceProvider.GetRequiredService<MainWindowViewModel>();
        this.MainWindow!.ViewModel = viewModel;
    }

    private void HandleParseError(IEnumerable<Error> errs)
    {
        foreach (var err in errs)
        {
            _logger.Error(err);
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
    }
}
