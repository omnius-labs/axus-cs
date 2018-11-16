using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Amoeba.Rpc;
using Amoeba.Service;
using CommandLine;
using Nett;
using Omnius.Base;

namespace Amoeba.Daemon
{
    sealed class SetupManager : DisposableBase
    {
        private TimerScheduler _timer;

        private readonly object _lockObject = new object();
        private volatile bool _disposed;

        public void Run()
        {
            // カレントディレクトリをexeと同じディレクトリパスへ変更。
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

            // ハンドルしていない例外をログ出力させる。
            AppDomain.CurrentDomain.UnhandledException += this.Program_UnhandledException;
            Thread.GetDomain().UnhandledException += this.Program_UnhandledException;

#if !DEBUG
            // Tomlファイルを読み込み。
            DaemonConfig config = null;
            {
                // コマンドライン引数を解析。
                var options = CommandLine.Parser.Default.ParseArguments<AmoebaDaemonOptions>(Environment.GetCommandLineArgs())
                    .MapResult(
                        (AmoebaDaemonOptions x) => x,
                        errs => null);
                if (options == null) return;

                if (File.Exists(options.ConfigFilePath))
                {
                    var tomlSettings = TomlSettings.Create(builder => builder
                        .ConfigureType<Version>(type => type
                            .WithConversionFor<TomlString>(convert => convert
                                .ToToml(tt => tt.ToString())
                                .FromToml(ft => Version.Parse(ft.Value)))));

                    config = Toml.ReadFile<DaemonConfig>(options.ConfigFilePath, tomlSettings);
                }

                if (config == null) return;
            }
#else
            DaemonConfig config = null;
            {
                var basePath = Environment.GetCommandLineArgs()[0];
                if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);

                config = new DaemonConfig(
                    new Version(0, 0, 0),
                    new DaemonConfig.CacheConfig(Path.Combine(basePath, "Cache.blocks")),
                    new DaemonConfig.PathsConfig(
                        Path.Combine(basePath, "Temp"),
                        Path.Combine(basePath, "Config/Service"),
                        Path.Combine(basePath, "Log")));
            }
#endif

            // 既定のフォルダを作成する。
            {
                foreach (var propertyInfo in typeof(DaemonConfig.PathsConfig).GetProperties())
                {
                    string path = propertyInfo.GetValue(config.Paths) as string;
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                }
            }

            // Tempフォルダを環境変数に登録。
            {
                // Tempフォルダ内を掃除。
                try
                {
                    foreach (string path in Directory.GetFiles(config.Paths.TempDirectoryPath, "*", SearchOption.AllDirectories))
                    {
                        File.Delete(path);
                    }

                    foreach (string path in Directory.GetDirectories(config.Paths.TempDirectoryPath, "*", SearchOption.AllDirectories))
                    {
                        Directory.Delete(path, true);
                    }
                }
                catch (Exception)
                {

                }

                Environment.SetEnvironmentVariable("TMP", Path.GetFullPath(config.Paths.TempDirectoryPath), EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("TEMP", Path.GetFullPath(config.Paths.TempDirectoryPath), EnvironmentVariableTarget.Process);
            }

            // ログファイルを設定する。
            this.Setting_Log(config);

            // 30分置きにLOHの圧縮を実行する。
            _timer = new TimerScheduler(() =>
            {
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();
            });
            _timer.Start(new TimeSpan(0, 30, 0));

            Socket targetSocket = null;

            // サービス開始。
            try
            {
#if !DEBUG
                for (int i = 50000; i < 60000; i++)
                {
                    try
                    {
                        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                        {
                            var endpoint = new IPEndPoint(IPAddress.Loopback, i);
                            socket.Bind(endpoint);
                            socket.Listen(1);

                            Console.Out.WriteLine(endpoint.ToString());

                            var sw = Stopwatch.StartNew();

                            for (; ; )
                            {
                                if (socket.Poll(0, SelectMode.SelectRead)) break;
                                if (sw.Elapsed.TotalSeconds > 30) return;

                                Thread.Sleep(1000);
                            }

                            targetSocket = socket.Accept();

                            break;
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
#else
                try
                {
                    using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        var endpoint = new IPEndPoint(IPAddress.Loopback, 60000);
                        socket.Bind(endpoint);
                        socket.Listen(1);

                        Console.Out.WriteLine(endpoint.ToString());

                        var sw = Stopwatch.StartNew();

                        for (; ; )
                        {
                            if (socket.Poll(0, SelectMode.SelectRead)) break;
                            if (sw.Elapsed.TotalSeconds > 30) return;

                            Thread.Sleep(1000);
                        }

                        targetSocket = socket.Accept();
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
#endif

                using (var bufferManager = new BufferManager(1024 * 1024 * 1024))
                using (var serviceManager = new ServiceManager(config.Paths.ConfigDirectoryPath, config.Cache.BlocksFilePath, bufferManager))
                using (var daemonManager = new AmoebaDaemonManager<ServiceManager>(targetSocket, serviceManager, bufferManager))
                {
                    try
                    {
                        daemonManager.Watch();
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);

                        Console.WriteLine(e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);

                Console.WriteLine(e.Message);
            }
            finally
            {
                if (targetSocket != null)
                {
                    targetSocket.Dispose();
                }
            }
        }

        private void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception == null) return;

            Log.Error(exception);
        }

        private void Setting_Log(DaemonConfig config)
        {
            var now = DateTime.Now;
            string logFilePath = null;
            bool isHeaderWriten = false;

            for (int i = 0; i < 1024; i++)
            {
                if (i == 0)
                {
                    logFilePath = Path.Combine(config.Paths.LogDirectoryPath,
                        string.Format("Daemon_{0}.txt", now.ToString("yyyy-MM-dd_HH-mm-ss", System.Globalization.DateTimeFormatInfo.InvariantInfo)));
                }
                else
                {
                    logFilePath = Path.Combine(config.Paths.LogDirectoryPath,
                        string.Format("Daemon_{0}.({1}).txt", now.ToString("yyyy-MM-dd_HH-mm-ss", System.Globalization.DateTimeFormatInfo.InvariantInfo), i));
                }

                if (!File.Exists(logFilePath)) break;
            }

            if (logFilePath == null) return;

            Log.MessageEvent += (sender, e) =>
            {
                if (e.Level == LogMessageLevel.Information) return;
#if !DEBUG
                if (e.Level == LogMessageLevel.Debug) return;
#endif

                lock (_lockObject)
                {
                    try
                    {
                        using (var writer = new StreamWriter(logFilePath, true, new UTF8Encoding(false)))
                        {
                            if (!isHeaderWriten)
                            {
                                writer.WriteLine(GetMachineInfomation());
                                isHeaderWriten = true;
                            }

                            writer.WriteLine(MessageToString(e));
                            writer.Flush();
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            };

            Log.ExceptionEvent += (sender, e) =>
            {
                if (e.Level == LogMessageLevel.Information) return;
#if !DEBUG
                if (e.Level == LogMessageLevel.Debug) return;
#endif

                lock (_lockObject)
                {
                    try
                    {
                        using (var writer = new StreamWriter(logFilePath, true, new UTF8Encoding(false)))
                        {
                            if (!isHeaderWriten)
                            {
                                writer.WriteLine(GetMachineInfomation());
                                isHeaderWriten = true;
                            }

                            writer.WriteLine(ExceptionToString(e));
                            writer.Flush();
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            };

            string MessageToString(LogMessageEventArgs e)
            {
                var sb = new StringBuilder();
                sb.AppendLine("--------------------------------------------------------------------------------");
                sb.AppendLine();
                sb.AppendLine($"Time: {DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}");
                sb.AppendLine($"Level: {e.Level}");
                sb.AppendLine($"Message: {e.Message}");

                sb.AppendLine();

                return sb.ToString();
            }

            string ExceptionToString(LogExceptionEventArgs e)
            {
                var sb = new StringBuilder();
                sb.AppendLine("--------------------------------------------------------------------------------");
                sb.AppendLine();
                sb.AppendLine($"Time: {DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}");
                sb.AppendLine($"Level: {e.Level}");

                var list = new List<Exception>();

                if (e.Exception is AggregateException aggregateException)
                {
                    list.AddRange(aggregateException.Flatten().InnerExceptions);
                }
                else
                {
                    var exception = e.Exception;

                    while (exception != null)
                    {
                        list.Add(exception);

                        try
                        {
                            exception = exception.InnerException;
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                }

                foreach (var exception in list)
                {
                    try
                    {
                        sb.AppendLine();
                        sb.AppendLine($"Exception: {exception.GetType().ToString()}");
                        if (!string.IsNullOrWhiteSpace(exception.Message)) sb.AppendLine($"Message: {exception.Message}");
                        if (!string.IsNullOrWhiteSpace(exception.StackTrace)) sb.AppendLine($"StackTrace: {exception.StackTrace}");
                    }
                    catch (Exception)
                    {

                    }
                }

                sb.AppendLine();

                return sb.ToString();
            }
        }

        private static string GetMachineInfomation()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Type: Daemon {Assembly.GetExecutingAssembly().GetName().Version}");
            sb.AppendLine($"OS: {RuntimeInformation.OSDescription}");
            sb.AppendLine($"Architecture: {RuntimeInformation.OSArchitecture}");
            sb.AppendLine($"Framework: {RuntimeInformation.FrameworkDescription}");

            return sb.ToString().Trim();
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }
    }
}
