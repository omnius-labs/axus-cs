using Avalonia;

namespace Omnius.Axis.Ui.Desktop;

public static class Program
{
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions { AllowEglInitialization = true })
            .LogToTrace();
}
