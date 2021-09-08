using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Omnius.Xeus.Ui.Desktop.Windows;

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
                string stateDirectoryPath = desktop.Args[0];

                try
                {
                    await Bootstrapper.Instance.BuildAsync(stateDirectoryPath);

                    desktop.MainWindow = new MainWindow()
                    {
                        ViewModel = Bootstrapper.Instance.ServiceProvider?.GetRequiredService<MainWindowViewModel>(),
                    };
                }
                finally
                {
                    await Bootstrapper.Instance.DisposeAsync();
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
