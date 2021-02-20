using System.Threading.Tasks;
using Cocona;

namespace Omnius.Xeus.Daemon
{
    public class Program : CoconaLiteConsoleAppBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            CoconaLiteApp.Run<Program>(args);
        }

        [Command("run")]
        public async ValueTask RunAsync([Option("state", new char[] { 's' })] string stateDirectoryPath = "../state/daemon")
        {
            _logger.Info("daemon start");

            using (var daemon = new Daemon())
            {
                await daemon.RunAsync(stateDirectoryPath, this.Context.CancellationToken);
            }

            _logger.Info("daemon end");
        }
    }
}
