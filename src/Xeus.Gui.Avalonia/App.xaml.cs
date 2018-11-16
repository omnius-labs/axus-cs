using Avalonia;
using Avalonia.Markup.Xaml;

namespace Xeus.Gui.Avalonia
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
