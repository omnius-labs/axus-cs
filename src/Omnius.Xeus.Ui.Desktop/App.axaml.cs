using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Omnius.Xeus.Ui.Desktop.Views.Windows.Main;

namespace Omnius.Xeus.Ui.Desktop
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static new App Current => (App)Application.Current;

        public IClassicDesktopStyleApplicationLifetime? Lifetime => (this.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime);

        public override async void OnFrameworkInitializationCompleted()
        {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
