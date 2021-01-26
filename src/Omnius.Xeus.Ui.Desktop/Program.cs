using Avalonia;
using Avalonia.ReactiveUI;

namespace Omnius.Xeus.Ui.Desktop
{
    public class Program
    {
        static void Main(string[] args)
        {
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .StartWithClassicDesktopLifetime(args);
        }
    }
}
