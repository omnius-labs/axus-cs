using System.Threading.Tasks;
using Omnius.Xeus.Engines.Models;
using Xunit;
using Moq;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using System.Net;
using System;
using Omnius.Core.Network;
using Omnius.Core;

namespace Omnius.Xeus.Engines.Connectors
{
    public class TcpConnectorTest
    {
        [Fact]
        public async Task ConnectAsyncSuccessTest()
        {
            int Port = 55555;
            var IpAddress = IPAddress.Parse("127.0.0.1");

            var tcpConnectingOptions = new TcpConnectingOptions(true, null);
            var tcpAcceptingOptions = new TcpAcceptingOptions(false, Array.Empty<OmniAddress>(), false);
            var bandwidthOptions = new BandwidthOptions(1024, 1024);
            var TcpConnectorOptions = new TcpConnectorOptions(tcpConnectingOptions, tcpAcceptingOptions, bandwidthOptions);

            var socks5ProxyClient = new Mock<ISocks5ProxyClient>();
            var socks5ProxyClientFactoryMock = new Mock<ISocks5ProxyClientFactory>();
            var httpProxyClientFactoryMock = new Mock<IHttpProxyClientFactory>();
            var upnpClientFactoryMock = new Mock<IUpnpClientFactory>();
            var bytesPool = BytesPool.Shared;

            var t = TcpConnector.Factory.CreateAsync(TcpConnectorOptions, socks5ProxyClientFactory, httpProxyClientFactory, upnpClientFactory, bytesPool);
        }

        [Fact]
        public async Task AcceptAsyncSuccessTest()
        {
        }
    }
}
