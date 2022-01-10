using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
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
            var basePath = Directory.GetCurrentDirectory();
            SetLogsDirectory(Path.Combine(basePath, "storage/launcher/logs"));

            _logger.Info("Starting...");
            _logger.Info("AssemblyInformationalVersion: {0}", Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);

            // gen bin path
            var daemonPath = Path.Combine(basePath, "bin/daemon/Omnius.Axis.Daemon");
            var uiDesktopPath = Path.Combine(basePath, "bin/ui-desktop/Omnius.Axis.Ui.Desktop");

            // add ext to suffix
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                daemonPath += ".exe";
                uiDesktopPath += ".exe";
            }

            // gen storage path
            var daemonStoragePath = Path.Combine(basePath, "storage/daemon");
            var uiDesktopStoragePath = Path.Combine(basePath, "storage/ui-desktop");

            // find free port
            var listenPort = FindFreeTcpPort();

            // start daemon
            var daemonProcessInfo = new ProcessStartInfo()
            {
                FileName = daemonPath,
                WorkingDirectory = Path.GetDirectoryName(daemonPath),
                Arguments = $"-l tcp(ip4(127.0.0.1),{listenPort}) -s {daemonStoragePath}",
                CreateNoWindow = true,
                UseShellExecute = false,
            };
            using var daemonProcess = Process.Start(daemonProcessInfo);

            // start ui-desktop
            var uiDesktopProcessInfo = new ProcessStartInfo()
            {
                FileName = uiDesktopPath,
                WorkingDirectory = Path.GetDirectoryName(uiDesktopPath),
                Arguments = $"-l tcp(ip4(127.0.0.1),{listenPort}) -s {uiDesktopStoragePath}",
                UseShellExecute = false,
            };
            using var uiDesktopProcess = Process.Start(uiDesktopProcessInfo);

            // wait for exit
            await uiDesktopProcess!.WaitForExitAsync();
            daemonProcess!.Kill();
            await daemonProcess!.WaitForExitAsync();
        }
        finally
        {
            _logger.Info("Stopping...");
            NLog.LogManager.Shutdown();
        }
    }

    private static int FindFreeTcpPort()
    {
        for (int i = 0; ; i++)
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
                _logger.Error(e);
                if (i >= 10) throw;
            }
        }
    }

    private static void SetLogsDirectory(string logsDirectoryPath)
    {
        var target = (NLog.Targets.FileTarget)NLog.LogManager.Configuration.FindTargetByName("log_file");
        target.FileName = $"{Path.GetFullPath(logsDirectoryPath)}/${{date:format=yyyy-MM-dd}}.log";
        target.ArchiveFileName = $"{Path.GetFullPath(logsDirectoryPath)}/logs/archive.{{#}}.log";
        NLog.LogManager.ReconfigExistingLoggers();
    }
}
