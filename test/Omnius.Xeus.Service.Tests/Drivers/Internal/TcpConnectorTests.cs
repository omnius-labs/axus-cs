using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Network;
using Xunit;

namespace Omnius.Xeus.Service.Drivers.Internal
{
    public class TcpConnectorTests
    {
        [Fact]
        public async Task ConnectAsyncTest()
        {
            const int Port = 55555;
            const string IpAddress = "127.0.0.1";

            var tcpConnectOptions = new TcpConnectOptions(true, null);
            var tcpAcceptOptions = new TcpAcceptOptions(false, Array.Empty<OmniAddress>(), false);

            await using (var connector = await TcpConnector.Factory.CreateAsync(tcpConnectOptions, tcpAcceptOptions, BytesPool.Shared))
            {
                var tcpListener = new TcpListener(IPAddress.Parse(IpAddress), Port);

                try
                {
                    tcpListener.Start();

                    var acceptTask = tcpListener.AcceptTcpClientAsync();

                    using var cap = await connector.ConnectAsync(new OmniAddress($"tcp(ip4(\"{IpAddress}\"),{Port})"));

                    await acceptTask;

                    Assert.NotNull(cap);

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
            var tcpConnectOptions = new TcpConnectOptions(false, null);
            var tcpAcceptOptions = new TcpAcceptOptions(true, new[] { new OmniAddress($"tcp(ip4(\"{ipAddress}\"),{port})") }, false);

            await using (var connector = await TcpConnector.Factory.CreateAsync(tcpConnectOptions, tcpAcceptOptions, BytesPool.Shared))
            {
                var acceptTask = connector.AcceptAsync();

                var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(IPAddress.Parse(ipAddress), port);

                await acceptTask;

                Assert.NotNull(acceptTask.Result.Item1);

                acceptTask.Result.Item1?.Dispose();
                tcpClient.Dispose();
            }
        }
    }
}
