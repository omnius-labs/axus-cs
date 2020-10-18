using System;
using System.Threading.Tasks;
using Moq;
using Omnius.Core;
using Omnius.Core.Network;
using Omnius.Core.Network.Connections;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Xeus.Engines.Models;
using Xunit;

namespace Omnius.Xeus.Engines.Connectors
{
    public class TcpConnectorTest
    {
        [Fact]
        public async Task ConnectAsyncSuccessTest()
        {
            TcpConnectorOptions tcpConnectorOptions;
            {
                var tcpConnectingOptions = new TcpConnectingOptions(true, null);
                var tcpAcceptingOptions = new TcpAcceptingOptions(false, Array.Empty<OmniAddress>(), false);
                var bandwidthOptions = new BandwidthOptions(1024, 1024);
                tcpConnectorOptions = new TcpConnectorOptions(tcpConnectingOptions, tcpAcceptingOptions, bandwidthOptions);
            }

            var socks5ProxyClientFactoryMock = new Mock<ISocks5ProxyClientFactory>();
            {
                var socks5ProxyClientMock = new Mock<ISocks5ProxyClient>();
                socks5ProxyClientFactoryMock.Setup(n => n.Create(It.IsAny<string>(), It.IsAny<int>())).Returns(socks5ProxyClientMock.Object);
            }

            var httpProxyClientFactoryMock = new Mock<IHttpProxyClientFactory>();
            var upnpClientFactoryMock = new Mock<IUpnpClientFactory>();
            var bytesPool = BytesPool.Shared;

            var tcpConnector = await TcpConnector.Factory.CreateAsync(tcpConnectorOptions, socks5ProxyClientFactoryMock.Object, httpProxyClientFactoryMock.Object, upnpClientFactoryMock.Object, bytesPool);
            var connectedConnection = await tcpConnector.ConnectAsync(OmniAddress.CreateTcpEndpoint("127.0.0.1", 55555), "test");
        }

        [Fact]
        public async Task AcceptAsyncSuccessTest()
        {
        }

        private async Task CommunicationTest(IConnection connection1, IConnection connection2)
        {

        }
    }
}
