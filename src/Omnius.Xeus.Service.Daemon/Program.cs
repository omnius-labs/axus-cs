using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cocona;
using Omnius.Core.Helpers;

namespace Omnius.Xeus.Service.Daemon
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
#if DEBUG
            ChangeLogLevel(NLog.LogLevel.Trace);
#endif

            try
            {
                _logger.Info("daemon start");
                await Runner.EventLoopAsync(stateDirectoryPath, this.Context.CancellationToken);
            }
            finally
            {
                _logger.Info("daemon stop");
                NLog.LogManager.Shutdown();
            }
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
    }
}
