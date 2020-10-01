using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Xeus.Components.Connectors;
using Omnius.Xeus.Components.Connectors.Primitives;
using Omnius.Xeus.Components.Engines;
using Omnius.Xeus.Components.Models;
using Omnius.Xeus.Daemon.Internal;
using Omnius.Xeus.Daemon.Models;
using Omnius.Xeus.Service;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Daemon
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
        public XeusServiceConfig? Config;
    }

    public class XeusService : IXeusService
    {
        private readonly XeusServiceOptions _options;

        private INodeFinder? _nodeFinder;

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
            var config = _options.Config ?? throw new ArgumentNullException();
            var tcpConnectorFactory = _options.TcpConnectorFactory ?? throw new ArgumentNullException();
            var socks5ProxyClientFactory = _options.Socks5ProxyClientFactory ?? throw new ArgumentNullException();
            var httpProxyClientFactory = _options.HttpProxyClientFactory ?? throw new ArgumentNullException();
            var upnpClientFactory = _options.UpnpClientFactory ?? throw new ArgumentNullException();
            var nodeFinderFactory = _options.NodeFinderFactory ?? throw new ArgumentNullException();
            var bytesPool = _options.BytesPool ?? throw new ArgumentNullException();
            var workingDirectory = _options.Config.WorkingDirectory ?? throw new ArgumentNullException();

            var connectors = new List<IConnector>();
            var tcpConnectorOptions = OptionsGenerator.GenTcpConnectorOptions(config);
            connectors.Add(await tcpConnectorFactory.CreateAsync(tcpConnectorOptions, socks5ProxyClientFactory, httpProxyClientFactory, upnpClientFactory, bytesPool));

            var nodeFinderOptions = new NodeFinderOptions(Path.Combine(workingDirectory, "node_finder"), 10);
            _nodeFinder = await nodeFinderFactory.CreateAsync(nodeFinderOptions, connectors, _options.BytesPool);
        }

        public async ValueTask AddCloudNodeProfilesAsync(AddCloudNodeProfilesParam param, CancellationToken cancellationToken)
        {
            if (_nodeFinder is null) throw new NullReferenceException(nameof(_nodeFinder));
            await _nodeFinder.AddCloudNodeProfilesAsync(param.NodeProfiles, cancellationToken);
        }

        public async ValueTask<FindNodeProfilesResult> FindNodeProfilesAsync(FindNodeProfilesParam param, CancellationToken cancellationToken)
        {
            if (_nodeFinder is null) throw new NullReferenceException(nameof(_nodeFinder));
            var result = await _nodeFinder.FindNodeProfilesAsync(param.ResourceTag, cancellationToken);
            return new FindNodeProfilesResult(result);
        }

        public async ValueTask GetMyNodeProfileAsync(CancellationToken cancellationToken)
        {
            if (_nodeFinder is null) throw new NullReferenceException(nameof(_nodeFinder));
            await _nodeFinder.GetMyNodeProfileAsync(cancellationToken);
        }
    }
}
