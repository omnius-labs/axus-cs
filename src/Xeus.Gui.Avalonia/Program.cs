using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using Xeus.Gui.Avalonia.ViewModels;
using Xeus.Gui.Avalonia.Views;

namespace Xeus.Gui.Avalonia
{
    class Program
    {
        static void Main(string[] args)
        {
            BuildAvaloniaApp().Start<MainWindow>(() => new MainWindowViewModel());
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .LogToDebug();
    }
}
