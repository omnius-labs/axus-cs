using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using Amoeba.Rpc;
using Amoeba.Service;
using Omnius.Base;

namespace Amoeba.Daemon
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var setup = new SetupManager())
            {
                setup.Run();
            }
        }
    }
}
