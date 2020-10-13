using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Xeus.Engines.Connectors;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Daemon.Internal;
using Omnius.Xeus.Daemon.Models;
using Omnius.Xeus.Api;
using Omnius.Xeus.Engines.Storages;
using Omnius.Xeus.Engines.Mediators;
using Omnius.Xeus.Engines.Exchangers;
using Omnius.Xeus.Api.Models;

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

        private ICkadMediator? _ckadMediator;
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
            var ckadMediatorFactory = _options.CkadMediatorFactory ?? throw new ArgumentNullException();
            var pushContentStorageFactory = _options.PushContentStorageFactory ?? throw new ArgumentNullException();
            var wantContentStorageFactory = _options.WantContentStorageFactory ?? throw new ArgumentNullException();
            var pushDeclaredMessageStorageFactory = _options.PushDeclaredMessageStorageFactory ?? throw new ArgumentNullException();
            var wantDeclaredMessageStorageFactory = _options.WantDeclaredMessageStorageFactory ?? throw new ArgumentNullException();
            var contentExchangerFactory = _options.ContentExchangerFactory ?? throw new ArgumentNullException();
            var declaredMessageExchangerFactory = _options.DeclaredMessageExchangerFactory ?? throw new ArgumentNullException();
            var bytesPool = _options.BytesPool ?? throw new ArgumentNullException();

            var tcpConnectorOptions = OptionsGenerator.GenTcpConnectorOptions(config);
            var tcpConnector = await tcpConnectorFactory.CreateAsync(tcpConnectorOptions, socks5ProxyClientFactory, httpProxyClientFactory, upnpClientFactory, bytesPool);

            var ckadMediatorOptions = OptionsGenerator.GenCkadMediatorOptions(config);
            _ckadMediator = await ckadMediatorFactory.CreateAsync(ckadMediatorOptions, new[] { tcpConnector }, _options.BytesPool);

            var pushContentStorageOptions = OptionsGenerator.GenPushContentStorageOptions(config);
            _pushContentStorage = await pushContentStorageFactory.CreateAsync(pushContentStorageOptions, bytesPool);

            var wantContentStorageOptions = OptionsGenerator.GenWantContentStorageOptions(config);
            _wantContentStorage = await wantContentStorageFactory.CreateAsync(wantContentStorageOptions, bytesPool);

            var pushDeclaredMessageStorageOptions = OptionsGenerator.GenPushDeclaredMessageStorageOptions(config);
            _pushDeclaredMessageStorage = await pushDeclaredMessageStorageFactory.CreateAsync(pushDeclaredMessageStorageOptions, bytesPool);

            var wantDeclaredMessageStorageOptions = OptionsGenerator.GenWantDeclaredMessageStorageOptions(config);
            _wantDeclaredMessageStorage = await wantDeclaredMessageStorageFactory.CreateAsync(wantDeclaredMessageStorageOptions, bytesPool);

            var contentExchangerOptions = OptionsGenerator.GenContentExchangerOptions(config);
            _contentExchanger = await contentExchangerFactory.CreateAsync(contentExchangerOptions, new[] { tcpConnector }, _ckadMediator, _pushContentStorage, _wantContentStorage, bytesPool);

            var declaredMessageExchangerOptions = OptionsGenerator.GenDeclaredMessageExchangerOptions(config);
            _declaredMessageExchanger = await declaredMessageExchangerFactory.CreateAsync(declaredMessageExchangerOptions, new[] { tcpConnector }, _ckadMediator, _pushDeclaredMessageStorage, _wantDeclaredMessageStorage, bytesPool);
        }

        public async ValueTask<GetMyNodeProfileResult> GetMyNodeProfileAsync(CancellationToken cancellationToken)
        {
            if (_ckadMediator is null) throw new NullReferenceException(nameof(_ckadMediator));
            var result = await _ckadMediator.GetMyNodeProfileAsync(cancellationToken);
            return new GetMyNodeProfileResult(result);
        }

        public async ValueTask AddCloudNodeProfilesAsync(AddCloudNodeProfilesParam param, CancellationToken cancellationToken)
        {
            if (_ckadMediator is null) throw new NullReferenceException(nameof(_ckadMediator));
            await _ckadMediator.AddCloudNodeProfilesAsync(param.NodeProfiles, cancellationToken);
        }

        public ValueTask<GetPushContentsReportResult> GetPushContentsReportAsync(CancellationToken cancellationToken)
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

        public ValueTask<GetWantContentsReportResult> GetWantContentsReportAsync(CancellationToken cancellationToken)
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

        public ValueTask ExportWantContentAsync(ExportWantContentParam param, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask<GetPushDeclaredMessagesReportResult> GetPushDeclaredMessagesReportAsync(CancellationToken cancellationToken)
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

        public ValueTask RegisterWantDeclaredMessageAsync(RegisterWantDeclaredMessageParam param, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask UnregisterWantDeclaredMessageAsync(UnregisterWantDeclaredMessageParam param, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask<ReadWantDeclaredMessageResult> ReadWantDeclaredMessageAsync(ReadWantDeclaredMessageParam param, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
