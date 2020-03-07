using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Network;
using Omnius.Xeus.Service.Primitives;
using Xunit;

namespace Omnius.Xeus.Service
{
    public class TcpConnectorTests : TestsBase
    {
        [Fact]
        public async Task ConnectAsyncTest()
        {
            const int port = 55555;
            const string ipAddress = "127.0.0.1";
            var options = new TcpConnectorOptions(new TcpConnectOptions(true, null), new TcpAcceptOptions(false, Array.Empty<OmniAddress>(), false));

            await using (var connector = await TcpConnector.Factory.CreateAsync(options, BytesPool.Shared))
            {
                var tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);

                try
                {
                    tcpListener.Start();

                    var acceptTask = tcpListener.AcceptTcpClientAsync();

                    var connectorResult = await connector.ConnectAsync(new OmniAddress($"tcp(ip4(\"{ipAddress}\"),{port})"));

                    await acceptTask;

                    Assert.Equal(ConnectorResultType.Succeeded, connectorResult.Type);

                    connectorResult.Cap?.Dispose();
                    tcpListener.Stop();
                }
                finally
                {
                    tcpListener.Server.Dispose();
                }
            }
        }

        [Fact]
        public async Task AcceptAsyncTest()
        {
            const int port = 55555;
            const string ipAddress = "127.0.0.1";
            var options = new TcpConnectorOptions(new TcpConnectOptions(false, null), new TcpAcceptOptions(true, new[] { new OmniAddress($"tcp(ip4(\"{ipAddress}\"),{port})") }, false));

            await using (var connector = await TcpConnector.Factory.CreateAsync(options, BytesPool.Shared))
            {
                var acceptTask = connector.AcceptAsync();

                var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(IPAddress.Parse(ipAddress), port);

                await acceptTask;

                Assert.Equal(ConnectorResultType.Succeeded, acceptTask.Result.Type);

                acceptTask.Result.Cap?.Dispose();
                tcpClient.Dispose();
            }
        }
    }
}
