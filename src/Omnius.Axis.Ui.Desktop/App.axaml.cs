using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Axis.Ui.Desktop.Views.Main;
using Omnius.Core.Avalonia;
using Omnius.Core.Helpers;
using Omnius.Core.Net;

namespace Omnius.Axis.Ui.Desktop;

public class App : Application
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    public static new App Current => (App)Application.Current;

    public new IClassicDesktopStyleApplicationLifetime ApplicationLifetime => (IClassicDesktopStyleApplicationLifetime)base.ApplicationLifetime;

    public override void Initialize()
    {
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.UnhandledException);

        if (!this.IsDesignMode)
        {
            var parsedResult = CommandLine.Parser.Default.ParseArguments<Options>(Environment.GetCommandLineArgs());
            parsedResult = parsedResult.WithParsed(this.Startup);
            parsedResult.WithNotParsed(this.HandleParseError);

            this.ApplicationLifetime.Exit += (_, _) => this.Exit();
        }

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new MainWindow();
            desktop.MainWindow = window;

            _ = this.InitMainWindowViewModelAsync(window);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async ValueTask InitMainWindowViewModelAsync(MainWindow mainWindow)
    {
        await Task.Delay(1).ConfigureAwait(false);

        var serviceProvider = Bootstrapper.Instance.GetServiceProvider();
        var applicationDispatcher = serviceProvider.GetRequiredService<IApplicationDispatcher>();
        var viewModel = serviceProvider.GetRequiredService<MainWindowViewModel>();

        await applicationDispatcher.InvokeAsync(() =>
        {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                mainWindow.ViewModel = viewModel;
            }
        });
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

    private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        _logger.Error(e);
    }

    private void Startup(Options options)
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

        Bootstrapper.Instance.Build(databaseDirectoryPath, OmniAddress.Parse(options.ListenAddress!));
    }

    private async void Exit()
    {
        await Bootstrapper.Instance.DisposeAsync();

        _logger.Info("Stopping...");
        NLog.LogManager.Shutdown();
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

    private void HandleParseError(IEnumerable<Error> errs)
    {
        foreach (var err in errs)
        {
            _logger.Error(err);
        }
    }
}
