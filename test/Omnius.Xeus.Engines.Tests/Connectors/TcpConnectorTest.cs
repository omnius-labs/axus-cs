using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Omnius.Core;
using Omnius.Core.Network;
using Omnius.Core.Network.Connections;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Core.RocketPack;
using Omnius.Xeus.Engines.Models;
using Xunit;

namespace Omnius.Xeus.Engines.Connectors
{
    public class TcpConnectorTest
    {
        [Fact]
        public async Task ConnectAsyncAndAcceptAsyncSuccessTest()
        {
            var connectingTcpConnector = await GenConnectingTcpConnector();
            var acceptingTcpConnector = await GenAcceptingTcpConnector();

            var connectedConnectionTask = ConnectAsync(connectingTcpConnector);
            var acceptedConnectionTask = AcceptAsync(acceptingTcpConnector);

            await acceptedConnectionTask;
            await connectedConnectionTask;

            await this.CommunicationTestAsync(connectedConnectionTask.Result, acceptedConnectionTask.Result);
            await this.CommunicationTestAsync(acceptedConnectionTask.Result, connectedConnectionTask.Result);

            async Task<IConnection> ConnectAsync(ITcpConnector connector)
            {
                var result = await connector.ConnectAsync(OmniAddress.CreateTcpEndpoint(IPAddress.Parse("127.0.0.1"), 55555), "test");
                Assert.NotNull(result);
                return result!;
            }

            async Task<IConnection> AcceptAsync(ITcpConnector connector)
            {
                var result = await connector.AcceptAsync("test");
                return result.Connection;
            }

            async Task<ITcpConnector> GenConnectingTcpConnector()
            {
                var tcpConnectingOptions = new TcpConnectingOptions(true, null);
                var tcpAcceptingOptions = new TcpAcceptingOptions(false, Array.Empty<OmniAddress>(), false);
                var bandwidthOptions = new BandwidthOptions(1024, 1024);
                var tcpConnectorOptions = new TcpConnectorOptions(tcpConnectingOptions, tcpAcceptingOptions, bandwidthOptions);

                var socks5ProxyClientFactory = GenSocks5ProxyClientFactory();
                var httpProxyClientFactory = GenHttpProxyClientFactory();
                var upnpClientFactory = GenUpnpClientFactory();
                var bytesPool = BytesPool.Shared;

                var tcpConnector = await TcpConnector.Factory.CreateAsync(tcpConnectorOptions, socks5ProxyClientFactory, httpProxyClientFactory, upnpClientFactory, bytesPool);
                return tcpConnector;
            }

            async Task<ITcpConnector> GenAcceptingTcpConnector()
            {
                var tcpConnectingOptions = new TcpConnectingOptions(false, null);
                var tcpAcceptingOptions = new TcpAcceptingOptions(true, new[] { OmniAddress.CreateTcpEndpoint(IPAddress.Parse("127.0.0.1"), 55555) }, false);
                var bandwidthOptions = new BandwidthOptions(1024 * 1024, 1024 * 1024);
                var tcpConnectorOptions = new TcpConnectorOptions(tcpConnectingOptions, tcpAcceptingOptions, bandwidthOptions);

                var socks5ProxyClientFactory = GenSocks5ProxyClientFactory();
                var httpProxyClientFactory = GenHttpProxyClientFactory();
                var upnpClientFactory = GenUpnpClientFactory();
                var bytesPool = BytesPool.Shared;

                var tcpConnector = await TcpConnector.Factory.CreateAsync(tcpConnectorOptions, socks5ProxyClientFactory, httpProxyClientFactory, upnpClientFactory, bytesPool);
                return tcpConnector;
            }

            static ISocks5ProxyClientFactory GenSocks5ProxyClientFactory()
            {
                var socks5ProxyClientMock = new Mock<ISocks5ProxyClient>();
                socks5ProxyClientMock.Setup(n => n.ConnectAsync(It.IsAny<Socket>(), It.IsAny<CancellationToken>())).Throws<Exception>();
                var socks5ProxyClientFactoryMock = new Mock<ISocks5ProxyClientFactory>();
                socks5ProxyClientFactoryMock.Setup(n => n.Create(It.IsAny<string>(), It.IsAny<int>())).Returns(socks5ProxyClientMock.Object);
                return socks5ProxyClientFactoryMock.Object;
            }

            static IHttpProxyClientFactory GenHttpProxyClientFactory()
            {
                var httpProxyClientMock = new Mock<IHttpProxyClient>();
                httpProxyClientMock.Setup(n => n.ConnectAsync(It.IsAny<Socket>(), It.IsAny<CancellationToken>())).Throws<Exception>();
                var httpProxyClientFactoryMock = new Mock<IHttpProxyClientFactory>();
                httpProxyClientFactoryMock.Setup(n => n.Create(It.IsAny<string>(), It.IsAny<int>())).Returns(httpProxyClientMock.Object);
                return httpProxyClientFactoryMock.Object;
            }

            static IUpnpClientFactory GenUpnpClientFactory()
            {
                var upnpClientMock = new Mock<IUpnpClient>();
                upnpClientMock.Setup(n => n.ConnectAsync(It.IsAny<CancellationToken>())).Throws<Exception>();
                var upnpClientFactoryMock = new Mock<IUpnpClientFactory>();
                upnpClientFactoryMock.Setup(n => n.Create()).Returns(upnpClientMock.Object);
                return upnpClientFactoryMock.Object;
            }
        }

        private async Task CommunicationTestAsync(IConnection sender, IConnection receiver)
        {
            const int seed = 0;
            const int loopCount = 10;

            async Task SendAsync(CancellationToken cancellationToken)
            {
                var random = new Random(seed);
                foreach (var i in Enumerable.Range(0, loopCount))
                {
                    var v = random.Next();
                    await sender.EnqueueAsync(writer => Varint.SetInt32(v, writer), cancellationToken);
                }
            }

            async Task ReceiveAsync(CancellationToken cancellationToken)
            {
                var random = new Random(seed);
                foreach (var i in Enumerable.Range(0, loopCount))
                {
                    int v = 0;
                    await receiver.DequeueAsync(sequence => Varint.TryGetInt32(ref sequence, out v), cancellationToken);

                    if (random.Next() != v) throw new Exception();
                }
            }

            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(20));

            var sendTask = SendAsync(cancellationTokenSource.Token);
            var receiveTask = ReceiveAsync(cancellationTokenSource.Token);

            await Task.WhenAll(sendTask, receiveTask);
        }
    }
}
