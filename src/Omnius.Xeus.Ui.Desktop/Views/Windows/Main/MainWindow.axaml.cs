using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Omnius.Core;
using Omnius.Core.Helpers;
using Omnius.Xeus.Ui.Desktop.Resources;
using Omnius.Xeus.Ui.Desktop.Views.Primitives;
using Omnius.Xeus.Ui.Desktop.Views.Windows.Main.Dashboard;

namespace Omnius.Xeus.Ui.Desktop.Views.Windows.Main
{
    public class MainWindow : StatefulWindowBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private AppState? _state;

        public MainWindow()
            : base()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override async ValueTask OnInitialize()
        {
            var args = App.Current.Lifetime!.Args ?? Array.Empty<string>();
            string stateDirectoryPath = args[0];
            string logsDirectoryPath = args[1];

            DirectoryHelper.CreateDirectory(stateDirectoryPath);
            DirectoryHelper.CreateDirectory(logsDirectoryPath);

            SetLogsDirectory(logsDirectoryPath);
            ChangeLogLevel();

            _logger.Info("desktop-ui start");

            var bytesPool = BytesPool.Shared;

            _state = await AppState.Factory.CreateAsync(stateDirectoryPath, bytesPool);

            this.Model = new MainWindowModel(_state);
            this.DashboardControl.Model = new DashboardControlModel(_state);
        }

        private static void SetLogsDirectory(string logsDirectoryPath)
        {
            var target = (NLog.Targets.FileTarget)NLog.LogManager.Configuration.FindTargetByName("log_file");
            target.FileName = $"{Path.GetFullPath(logsDirectoryPath)}/${{date:format=yyyy-MM-dd}}.log";
            target.ArchiveFileName = $"{Path.GetFullPath(logsDirectoryPath)}/logs/archive.{{#}}.log";
            NLog.LogManager.ReconfigExistingLoggers();
        }

        private static void ChangeLogLevel()
        {
#if DEBUG
            var rootLoggingRule = NLog.LogManager.Configuration.LoggingRules.First(n => n.NameMatches("*"));
            rootLoggingRule.EnableLoggingForLevel(NLog.LogLevel.Trace);
#endif
        }

        protected override async ValueTask OnDispose()
        {
            if (this.DashboardControl.Model is DashboardControlModel dashboardControlModel)
            {
                await dashboardControlModel.DisposeAsync();
            }

            if (this.Model is MainWindowModel mainWindowModel)
            {
                await mainWindowModel.DisposeAsync();
            }

            if (_state is not null)
            {
                await _state.DisposeAsync();
            }

            _logger.Info("desktop-ui end");

            NLog.LogManager.Shutdown();
        }

        public MainWindowModel? Model
        {
            get => this.DataContext as MainWindowModel;
            set => this.DataContext = value;
        }

        public DashboardControl DashboardControl => this.FindControl<DashboardControl>(nameof(this.DashboardControl));
    }
}
