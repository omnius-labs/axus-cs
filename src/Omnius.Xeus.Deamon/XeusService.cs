using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Xeus.Components.Connectors;
using Omnius.Xeus.Components.Engines;
using Omnius.Xeus.Components.Models;
using Omnius.Xeus.Deamon.Models;
using Omnius.Xeus.Rpc;

namespace Omnius.Xeus.Deamon
{
    public record XeusServiceOptions
    {
        public ISocks5ProxyClientFactory? Socks5ProxyClientFactory;
        public IHttpProxyClientFactory? HttpProxyClientFactory;
        public IUpnpClientFactory? UpnpClientFactory;
        public ITcpConnectorFactory? TcpConnectorFactory;
        public INodeFinderFactory? NodeFinderFactory;
        public IContentExchangerFactory? ContentExchangerFactory;
        public IDeclaredMessageExchangerFactory? DeclaredMessageExchangerFactory;
        public IBytesPool? BytesPool;
        public XeusConfig? Config;
    }

    public class XeusService : IXeusService
    {
        private readonly XeusServiceOptions _options;

        private INodeFinder _nodeFinder;

        public static async ValueTask<XeusService> CreateAsync(XeusServiceOptions options)
        {
            var service = new XeusService(options);
            await service.InitAsync();
            return service;
        }

        private XeusService(XeusServiceOptions options)
        {
            _options = options;
        }

        private async ValueTask InitAsync(CancellationToken cancellationToken = default)
        {
            var connectors = new List<IConnector>();
            var tcpConnectingOptions = new TcpConnectingOptions();
            var tcpAcceptingOptions = new TcpAcceptingOptions();
            var bandwidthOptions = new BandwidthOptions();
            var tcpConnectorOptions = new TcpConnectorOptions(tcpConnectingOptions, tcpAcceptingOptions, bandwidthOptions);
            connectors.Add(await _options.TcpConnectorFactory.CreateAsync(tcpConnectorOptions, _options.Socks5ProxyClientFactory, _options.HttpProxyClientFactory, _options.UpnpClientFactory, _options.BytesPool));

            var nodeFinderOptions = new NodeFinderOptions(Path.Combine(_options.Config.WorkingDirectory, "node_finder"), 10);
            _nodeFinder = await _options.NodeFinderFactory.CreateAsync(nodeFinderOptions, connectors, _options.BytesPool);
        }

        public ValueTask<AddCloudNodeProfilesResult> AddCloudNodeProfilesAsync(CancellationToken cancellationToken)
        {

        }

        public ValueTask<FindNodeProfilesResult> FindNodeProfilesAsync(FindNodeProfilesParam param, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask GetMyNodeProfileAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
