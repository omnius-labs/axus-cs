using Avalonia;
using Avalonia.Logging.Serilog;
using Xeus.Gui.Desktop.Windows;

namespace Xeus.Gui.Desktop
{
    class Program
    {
        static void Main(string[] args)
        {
            BuildAvaloniaApp().Start<MainWindow>();
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug();
    }
}
