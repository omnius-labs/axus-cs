using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cocona;
using Omnius.Core.Helpers;

namespace Omnius.Xeus.Daemon
{
    public class Program : CoconaLiteConsoleAppBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            CoconaLiteApp.Run<Program>(args);
        }

        public async ValueTask RunAsync([Argument("state")] string stateDirectoryPath = "../daemon/state", [Argument("logs")] string logsDirectoryPath = "../daemon/logs")
        {
            DirectoryHelper.CreateDirectory(stateDirectoryPath);
            DirectoryHelper.CreateDirectory(logsDirectoryPath);

            SetLogsDirectory(logsDirectoryPath);
            ChangeLogLevel();

            try
            {
                _logger.Info("daemon start");

                using (var daemon = new Daemon())
                {
                    await daemon.RunAsync(stateDirectoryPath, this.Context.CancellationToken);
                }

                _logger.Info("daemon end");
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        private static void SetLogsDirectory(string logsDirectoryPath)
        {
            var target = (NLog.Targets.FileTarget)NLog.LogManager.Configuration.FindTargetByName("log_file");
            target.FileName = $"{Path.GetFullPath(logsDirectoryPath)}/${{date:format=yyyy-MM-dd_HH-mm-ss}}.log";
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
    }
}
