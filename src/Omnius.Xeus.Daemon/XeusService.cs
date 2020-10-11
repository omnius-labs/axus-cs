using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Xeus.Api;
using Omnius.Xeus.Daemon.Internal;
using Omnius.Xeus.Daemon.Models;
using Omnius.Xeus.Engines.Connectors;
using Omnius.Xeus.Engines.Connectors.Primitives;
using Omnius.Xeus.Engines.Engines;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages;

namespace Omnius.Xeus.Daemon
{
    public record XeusServiceOptions
    {
        public ISocks5ProxyClientFactory? Socks5ProxyClientFactory;
        public IHttpProxyClientFactory? HttpProxyClientFactory;
        public IUpnpClientFactory? UpnpClientFactory;
        public ITcpConnectorFactory? TcpConnectorFactory;
        public ICkadMediatorFactory? CkadMediatorFactory;
        public IContentExchangerFactory? ContentExchangerFactory;
        public IDeclaredMessageExchangerFactory? DeclaredMessageExchangerFactory;
        public IPushContentStorageFactory? PushContentStorageFactory;
        public IWantContentStorageFactory? WantContentStorageFactory;
        public IPushDeclaredMessageStorageFactory? PushDeclaredMessageStorageFactory;
        public IWantDeclaredMessageStorageFactory? WantDeclaredMessageStorageFactory;
        public IBytesPool? BytesPool;
        public XeusServiceConfig? Config;
    }

    public class XeusService : IXeusService
    {
        private readonly XeusServiceOptions _options;

        private ICkadMediator? _nodeFinder;
        public IContentExchanger? _contentExchanger;
        public IDeclaredMessageExchanger? _declaredMessageExchanger;
        public IPushContentStorage? _pushContentStorage;
        public IWantContentStorage? _wantContentStorage;
        public IPushDeclaredMessageStorage? _pushDeclaredMessageStorage;
        public IWantDeclaredMessageStorage? _wantDeclaredMessageStorage;

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

        private async ValueTask InitAsync()
        {
            var config = _options.Config ?? throw new ArgumentNullException();
            var tcpConnectorFactory = _options.TcpConnectorFactory ?? throw new ArgumentNullException();
            var socks5ProxyClientFactory = _options.Socks5ProxyClientFactory ?? throw new ArgumentNullException();
            var httpProxyClientFactory = _options.HttpProxyClientFactory ?? throw new ArgumentNullException();
            var upnpClientFactory = _options.UpnpClientFactory ?? throw new ArgumentNullException();
            var nodeFinderFactory = _options.CkadMediatorFactory ?? throw new ArgumentNullException();
            var pushContentStorageFactory = _options.PushContentStorageFactory ?? throw new ArgumentNullException();
            var wantContentStorageFactory = _options.WantContentStorageFactory ?? throw new ArgumentNullException();
            var pushDeclaredMessageStorageFactory = _options.PushDeclaredMessageStorageFactory ?? throw new ArgumentNullException();
            var wantDeclaredMessageStorageFactory = _options.WantDeclaredMessageStorageFactory ?? throw new ArgumentNullException();
            var bytesPool = _options.BytesPool ?? throw new ArgumentNullException();
            var workingDirectory = _options.Config.WorkingDirectory ?? throw new ArgumentNullException();

            var connectors = new List<IConnector>();
            var tcpConnectorOptions = OptionsGenerator.GenTcpConnectorOptions(config);
            connectors.Add(await tcpConnectorFactory.CreateAsync(tcpConnectorOptions, socks5ProxyClientFactory, httpProxyClientFactory, upnpClientFactory, bytesPool));

            var nodeFinderOptions = new CkadMediatorOptions(Path.Combine(workingDirectory, "node_finder"), 10);
            _nodeFinder = await nodeFinderFactory.CreateAsync(nodeFinderOptions, connectors, _options.BytesPool);

            var pushContentStorage = _options.PushContentStorageFactory.CreateAsync();
            var wantContentStorage = _options.WantContentStorageFactory.CreateAsync();
            var pushDeclaredMessageStorage = _options.PushDeclaredMessageStorageFactory.CreateAsync();
            var wantDeclaredMessageStorage = _options.WantDeclaredMessageStorageFactory.CreateAsync();
        }

        public async ValueTask<GetMyNodeProfileResult> GetMyNodeProfileAsync(CancellationToken cancellationToken)
        {
            if (_nodeFinder is null) throw new NullReferenceException(nameof(_nodeFinder));
            var result = await _nodeFinder.GetMyNodeProfileAsync(cancellationToken);
            return new GetMyNodeProfileResult(result);
        }

        public async ValueTask AddCloudNodeProfilesAsync(AddCloudNodeProfilesParam param, CancellationToken cancellationToken)
        {
            if (_nodeFinder is null) throw new NullReferenceException(nameof(_nodeFinder));
            await _nodeFinder.AddCloudNodeProfilesAsync(param.NodeProfiles, cancellationToken);
        }

        public ValueTask<GetPushContentStorageReportResult> GetPushContentStorageReportAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask<RegisterPushContentResult> RegisterPushContentAsync(RegisterPushContentParam param, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask UnregisterPushContentAsync(UnregisterPushContentParam param, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask<GetWantContentStorageReportResult> GetWantContentStorageReportAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask RegisterWantContentAsync(RegisterWantContentParam param, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask UnregisterWantContentAsync(UnregisterWantContentParam param, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask<ExportContentResult> ExportContentAsync(ExportContentParam param, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask<GetPushDeclaredMessageStorageReportResult> GetPushDeclaredMessageStorageReportAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask RegisterPushDeclaredMessageAsync(RegisterPushDeclaredMessageParam param, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask UnregisterPushDeclaredMessageAsync(UnregisterPushDeclaredMessageParam param, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask<GetWantDeclaredMessageStorageReportResult> GetWantDeclaredMessageStorageReportAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask RegisterWantDeclaredMessageAsync(RegisterWantDeclaredMessageParam param, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask UnregisterWantDeclaredMessageAsync(UnregisterWantDeclaredMessageParam param, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask<ReadDeclaredMessageResult> ReadDeclaredMessageAsync(ReadDeclaredMessageParam param, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
