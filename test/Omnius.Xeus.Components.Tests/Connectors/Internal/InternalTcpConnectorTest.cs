using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Network;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Xeus.Engines.Models;
using Xunit;

namespace Omnius.Xeus.Engines.Connectors.Internal
{
    public class InternalTcpConnectorTest
    {
        /// <summary>
        /// InternalTcpConnectorのConnectAsyncが成功することを確認する
        /// </summary>
        [Fact]
        public async Task ConnectAsyncSuccessTest()
        {
            const int Port = 55555;
            var IpAddress = IPAddress.Parse("127.0.0.1");

            var tcpConnectingOptions = new TcpConnectingOptions(true, null);
            var tcpAcceptingOptions = new TcpAcceptingOptions(false, Array.Empty<OmniAddress>(), false);

            await using (var connector = await InternalTcpConnector.Factory.CreateAsync(tcpConnectingOptions, tcpAcceptingOptions, Socks5ProxyClient.Factory, HttpProxyClient.Factory, UpnpClient.Factory, BytesPool.Shared))
            {
                var tcpListener = new TcpListener(IpAddress, Port);

                try
                {
                    tcpListener.Start();

                    var acceptTask = tcpListener.AcceptTcpClientAsync();

                    using var cap = await connector.ConnectAsync(OmniAddress.CreateTcpEndpoint(IpAddress, Port));

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

        /// <summary>
        /// InternalTcpConnectorのAcceptAsyncが成功することを確認する
        /// </summary>
        [Fact]
        public async Task AcceptAsyncSuccessTest()
        {
            const int port = 55555;
            var ipAddress = IPAddress.Parse("127.0.0.1");
            var tcpConnectingOptions = new TcpConnectingOptions(false, null);
            var tcpAcceptingOptions = new TcpAcceptingOptions(true, new[] { OmniAddress.CreateTcpEndpoint(ipAddress, port) }, false);

            await using (var connector = await InternalTcpConnector.Factory.CreateAsync(tcpConnectingOptions, tcpAcceptingOptions, Socks5ProxyClient.Factory, HttpProxyClient.Factory, UpnpClient.Factory, BytesPool.Shared))
            {
                var acceptTask = connector.AcceptAsync();

                var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(ipAddress, port);

                await acceptTask;

                Assert.NotNull(acceptTask.Result.Item1);

                acceptTask.Result.Item1?.Dispose();
                tcpClient.Dispose();
            }
        }
    }
}
