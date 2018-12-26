using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;

namespace Lxna.Gui.Wpf.Internal
{
    class LxnaEnvironment
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static Version Version { get; private set; }
        public static PathsEnvironment Paths { get; private set; }
        public static IconsEnvironment Icons { get; private set; }
        public static ImagesEnvironment Images { get; private set; }

        static LxnaEnvironment()
        {
            try
            {
                Version = new Version(1, 0, 0);
                Paths = new PathsEnvironment();
                Icons = new IconsEnvironment();
                Images = new ImagesEnvironment();
            }
            catch (Exception e)
            {
                _logger.Error(e, "LxnaEnvironment static constructor.");
            }
        }

        public class PathsEnvironment
        {
            public string BinDirectoryPath { get; private set; }
            public string InterfaceDirectoryPath { get; private set; }
            public string BaseDirectoryPath { get; private set; }
            public string TempDirectoryPath { get; private set; }
            public string ConfigDirectoryPath { get; private set; }
            public string LogsDirectoryPath { get; private set; }
            public string WorkDirectoryPath { get; private set; }
            public string LanguagesDirectoryPath { get; private set; }
            public string IconsDirectoryPath { get; private set; }
            public string DaemonDirectoryPath { get; private set; }

            public PathsEnvironment()
            {
                this.BinDirectoryPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                this.InterfaceDirectoryPath = Path.GetDirectoryName(this.BinDirectoryPath);
                this.BaseDirectoryPath = Path.GetDirectoryName(this.InterfaceDirectoryPath);
                this.TempDirectoryPath = Path.Combine(this.InterfaceDirectoryPath, "Temp");
                this.ConfigDirectoryPath = Path.Combine(this.InterfaceDirectoryPath, "Config");
                this.LogsDirectoryPath = Path.Combine(this.InterfaceDirectoryPath, "Logs");
                this.WorkDirectoryPath = Path.Combine(this.InterfaceDirectoryPath, "Work");
                this.LanguagesDirectoryPath = Path.Combine(this.BinDirectoryPath, "Resources/Languages");
                this.IconsDirectoryPath = Path.Combine(this.BinDirectoryPath, "Resources/Icons");
                this.DaemonDirectoryPath = Path.Combine(this.BaseDirectoryPath, "Deamon");
            }
        }

        public class IconsEnvironment
        {
            public BitmapImage Amoeba { get; }

            public IconsEnvironment()
            {
                this.Amoeba = GetIcon("Lxna.ico");
           }

            private static BitmapImage GetIcon(string path)
            {
                try
                {
                    var icon = new BitmapImage();

                    icon.BeginInit();
                    icon.StreamSource = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "Resources/Icons/", path), FileMode.Open, FileAccess.Read, FileShare.Read);
                    icon.EndInit();
                    if (icon.CanFreeze) icon.Freeze();

                    return icon;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public class ImagesEnvironment
        {
            public BitmapImage Lxna { get; }
            public BitmapImage BlueBall { get; }
            public BitmapImage GreenBall { get; }
            public BitmapImage YelloBall { get; }

            public ImagesEnvironment()
            {
                this.Lxna = GetImage("Lxna.png");
                this.BlueBall = GetImage("States/Blue.png");
                this.GreenBall = GetImage("States/Green.png");
                this.YelloBall = GetImage("States/Yello.png");
            }

            private static BitmapImage GetImage(string path)
            {
                try
                {
                    var icon = new BitmapImage();

                    icon.BeginInit();
                    icon.StreamSource = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "Resources/Images/", path), FileMode.Open, FileAccess.Read, FileShare.Read);
                    icon.EndInit();
                    if (icon.CanFreeze) icon.Freeze();

                    return icon;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
