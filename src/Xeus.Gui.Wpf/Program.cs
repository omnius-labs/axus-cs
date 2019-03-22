using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lxna.Gui.Wpf
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            App app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
