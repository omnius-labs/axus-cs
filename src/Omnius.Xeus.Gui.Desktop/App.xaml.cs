using Avalonia;
using Avalonia.Markup.Xaml;

namespace Omnius.Xeus.Gui.Desktop
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
