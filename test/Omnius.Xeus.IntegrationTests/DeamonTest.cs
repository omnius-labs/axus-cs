using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Test;
using Omnius.Core.Network;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Xeus.Components.Models;
using Omnius.Xeus.Deamon;
using Xunit;

namespace Omnius.Xeus.IntegrationTests
{
    public class TcpConnectorTest
    {
        [Fact]
        public async Task ConnectAsyncSuccessTest()
        {
            Omnius.Core.Test.TestEnvironment.GetBasePath();
        }
    }
}
