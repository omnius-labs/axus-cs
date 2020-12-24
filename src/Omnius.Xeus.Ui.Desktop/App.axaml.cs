using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Omnius.Xeus.Ui.Desktop.ViewModels;
using Omnius.Xeus.Ui.Desktop.Views;

namespace Omnius.Xeus.Ui.Desktop
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindowViewModel = new MainWindowViewModel();

                desktop.MainWindow = new MainWindow() { ViewModel = mainWindowViewModel };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
