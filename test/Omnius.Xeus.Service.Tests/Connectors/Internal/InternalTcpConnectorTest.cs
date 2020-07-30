using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Network;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Xeus.Service.Models;
using Xunit;

namespace Omnius.Xeus.Service.Connectors.Internal
{
    public class InternalTcpConnectorTest
    {
        /// <summary>
        /// InternalTcpConnectorのConnectAsyncが成功することを確認する
        /// </summary>
        [Fact]
        public async Task ConnectAsyncSuccessTestAsync()
        {
            const int Port = 55555;
            const string IpAddress = "127.0.0.1";

            var tcpConnectingOptions = new TcpConnectingOptions(true, null);
            var tcpAcceptingOptions = new TcpAcceptingOptions(false, Array.Empty<OmniAddress>(), false);

            await using (var connector = await InternalTcpConnector.Factory.CreateAsync(tcpConnectingOptions, tcpAcceptingOptions, Socks5ProxyClient.Factory, HttpProxyClient.Factory, UpnpClient.Factory, BytesPool.Shared))
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

        /// <summary>
        /// InternalTcpConnectorのAcceptAsyncが成功することを確認する
        /// </summary>
        [Fact]
        public async Task AcceptAsyncSuccessTestAsync()
        {
            const int port = 55555;
            const string ipAddress = "127.0.0.1";
            var tcpConnectingOptions = new TcpConnectingOptions(false, null);
            var tcpAcceptingOptions = new TcpAcceptingOptions(true, new[] { new OmniAddress($"tcp(ip4(\"{ipAddress}\"),{port})") }, false);

            await using (var connector = await InternalTcpConnector.Factory.CreateAsync(tcpConnectingOptions, tcpAcceptingOptions, Socks5ProxyClient.Factory, HttpProxyClient.Factory, UpnpClient.Factory, BytesPool.Shared))
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
