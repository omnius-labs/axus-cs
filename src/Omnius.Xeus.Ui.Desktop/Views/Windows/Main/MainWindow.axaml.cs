using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Omnius.Core;
using Omnius.Core.Helpers;
using Omnius.Xeus.Ui.Desktop.Resources;
using Omnius.Xeus.Ui.Desktop.Views.Windows.Main.Download;
using Omnius.Xeus.Ui.Desktop.Views.Windows.Main.Peers;
using Omnius.Xeus.Ui.Desktop.Views.Windows.Main.Settings;
using Omnius.Xeus.Ui.Desktop.Views.Windows.Main.Status;
using Omnius.Xeus.Ui.Desktop.Views.Windows.Main.Upload;
using Omnius.Xeus.Ui.Desktop.Views.Windows.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Views.Windows.Main
{
    public partial class MainWindow : StatefulWindowBase
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
#if DEBUG
            ChangeLogLevel(NLog.LogLevel.Trace);
#endif

            _logger.Info("desktop-ui start");

            var bytesPool = BytesPool.Shared;

            _state = await AppState.Factory.CreateAsync(stateDirectoryPath, bytesPool);

            this.Model = new MainWindowModel(_state);
            this.StatusControl.Model = new StatusControlModel(_state);
            this.PeersControl.Model = new PeersControlModel(_state);
            this.DownloadControl.Model = new DownloadControlModel(_state);
            this.UploadControl.Model = new UploadControlModel(_state);
            this.SettingsControl.Model = new SettingsControlModel(_state);
        }

        private static void SetLogsDirectory(string logsDirectoryPath)
        {
            var target = (NLog.Targets.FileTarget)NLog.LogManager.Configuration.FindTargetByName("log_file");
            target.FileName = $"{Path.GetFullPath(logsDirectoryPath)}/${{date:format=yyyy-MM-dd}}.log";
            target.ArchiveFileName = $"{Path.GetFullPath(logsDirectoryPath)}/logs/archive.{{#}}.log";
            NLog.LogManager.ReconfigExistingLoggers();
        }

        private static void ChangeLogLevel(NLog.LogLevel logLevel)
        {
            var rootLoggingRule = NLog.LogManager.Configuration.LoggingRules.First(n => n.NameMatches("*"));
            rootLoggingRule.EnableLoggingForLevel(logLevel);
        }

        protected override async ValueTask OnDispose()
        {
            if (this.StatusControl.Model is StatusControlModel statusControlModel)
            {
                await statusControlModel.DisposeAsync();
            }

            if (this.PeersControl.Model is PeersControlModel peersControlModel)
            {
                await peersControlModel.DisposeAsync();
            }

            if (this.DownloadControl.Model is DownloadControlModel downloadControlModel)
            {
                await downloadControlModel.DisposeAsync();
            }

            if (this.UploadControl.Model is UploadControlModel uploadControlModel)
            {
                await uploadControlModel.DisposeAsync();
            }

            if (this.SettingsControl.Model is SettingsControlModel settingsControlModel)
            {
                await settingsControlModel.DisposeAsync();
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

        public IMainWindowModel? Model
        {
            get => this.DataContext as IMainWindowModel;
            set => this.DataContext = value;
        }

        public StatusControl StatusControl => this.FindControl<StatusControl>(nameof(this.StatusControl));

        public PeersControl PeersControl => this.FindControl<PeersControl>(nameof(this.PeersControl));

        public DownloadControl DownloadControl => this.FindControl<DownloadControl>(nameof(this.DownloadControl));

        public UploadControl UploadControl => this.FindControl<UploadControl>(nameof(this.UploadControl));

        public SettingsControl SettingsControl => this.FindControl<SettingsControl>(nameof(this.SettingsControl));
    }
}
