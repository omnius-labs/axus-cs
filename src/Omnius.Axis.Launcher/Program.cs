using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace Omnius.Axis.Launcher;

public class Program
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    public static async Task Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program.UnhandledException);
        await RunAsync();
    }

    private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        _logger.Error(e);
    }

    public static async ValueTask RunAsync()
    {
        try
        {
            SetLogsDirectory("./storage/launcher/logs");

            _logger.Info("Starting...");

            var basePath = Directory.GetCurrentDirectory();

            // gen path
            var daemonPath = Path.Combine(basePath, "bin/daemon/Omnius.Axis.Daemon");
            var uiDesktopPath = Path.Combine(basePath, "bin/ui-desktop/Omnius.Axis.Ui.Desktop");

            // add ext to suffix
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                daemonPath += ".exe";
                uiDesktopPath += ".exe";
            }

            // find free port
            var listenPort = FindFreePort();

            // start daemon
            var daemonProcessInfo = new ProcessStartInfo()
            {
                FileName = daemonPath,
                Arguments = $"-l tcp(ip4(127.0.0.1),{listenPort}) -s ./storage/daemon/",
                WorkingDirectory = Path.GetDirectoryName(daemonPath),
                UseShellExecute = false,
            };
            using var daemonProcess = Process.Start(daemonProcessInfo);

            // start ui-desktop
            var uiDesktopProcessInfo = new ProcessStartInfo()
            {
                FileName = uiDesktopPath,
                Arguments = $"-l tcp(ip4(127.0.0.1),{listenPort}) -s ./storage/ui-desktop/",
                WorkingDirectory = Path.GetDirectoryName(uiDesktopPath),
                UseShellExecute = false,
            };
            using var uiDesktopProcess = Process.Start(uiDesktopProcessInfo);

            // wait for exit
            await daemonProcess!.WaitForExitAsync();
            await uiDesktopProcess!.WaitForExitAsync();
        }
        finally
        {
            _logger.Info("Stopping...");
            NLog.LogManager.Shutdown();
        }
    }

    private static int FindFreePort()
    {
        Exception? lastException = null;

        for (int i = 0; i < 10; i++)
        {
            try
            {
                var port = Random.Shared.Next(10000, 60000);
                using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
                socket.Close();

                return port;
            }
            catch (Exception e)
            {
                lastException = e;
            }
        }

        _logger.Error(lastException);
        ExceptionDispatchInfo.Throw(lastException!);

        return 0; // Not executed
    }

    private static void SetLogsDirectory(string logsDirectoryPath)
    {
        var target = (NLog.Targets.FileTarget)NLog.LogManager.Configuration.FindTargetByName("log_file");
        target.FileName = $"{Path.GetFullPath(logsDirectoryPath)}/${{date:format=yyyy-MM-dd}}.log";
        target.ArchiveFileName = $"{Path.GetFullPath(logsDirectoryPath)}/logs/archive.{{#}}.log";
        NLog.LogManager.ReconfigExistingLoggers();
    }
}
