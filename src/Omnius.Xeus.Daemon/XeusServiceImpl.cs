using System;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Xeus.Api;
using Omnius.Xeus.Daemon.Configs;
using Omnius.Xeus.Daemon.Internal;
using Omnius.Xeus.Engines.Connectors;
using Omnius.Xeus.Engines.Exchangers;
using Omnius.Xeus.Engines.Mediators;
using Omnius.Xeus.Engines.Storages;

namespace Omnius.Xeus.Daemon
{
    public class XeusServiceOptions
    {
        public ISocks5ProxyClientFactory? Socks5ProxyClientFactory { get; init; }

        public IHttpProxyClientFactory? HttpProxyClientFactory { get; init; }

        public IUpnpClientFactory? UpnpClientFactory { get; init; }

        public ITcpConnectorFactory? TcpConnectorFactory { get; init; }

        public ICkadMediatorFactory? CkadMediatorFactory { get; init; }

        public IContentExchangerFactory? ContentExchangerFactory { get; init; }

        public IDeclaredMessageExchangerFactory? DeclaredMessageExchangerFactory { get; init; }

        public IContentPublisherFactory? ContentPublisherFactory { get; init; }

        public IContentSubscriberFactory? ContentSubscriberFactory { get; init; }

        public IDeclaredMessagePublisherFactory? DeclaredMessagePublisherFactory { get; init; }

        public IDeclaredMessageSubscriberFactory? DeclaredMessageSubscriberFactory { get; init; }

        public IBytesPool? BytesPool { get; init; }

        public EnginesConfig? Config { get; init; }
    }

    public class XeusServiceImpl : IXeusService
    {
        private readonly XeusServiceOptions _options;

        private ICkadMediator _ckadMediator = null!;
        private IContentExchanger _contentExchanger = null!;
        private IDeclaredMessageExchanger _declaredMessageExchanger = null!;
        private IContentPublisher _contentPublisher = null!;
        private IContentSubscriber _contentSubscriber = null!;
        private IDeclaredMessagePublisher _declaredMessagePublisher = null!;
        private IDeclaredMessageSubscriber _declaredMessageSubscriber = null!;

        public static async ValueTask<XeusServiceImpl> CreateAsync(XeusServiceOptions options)
        {
            var service = new XeusServiceImpl(options);
            await service.InitAsync();

            return service;
        }

        private XeusServiceImpl(XeusServiceOptions options)
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
            var contentPublisherFactory = _options.ContentPublisherFactory ?? throw new ArgumentNullException();
            var contentSubscriberFactory = _options.ContentSubscriberFactory ?? throw new ArgumentNullException();
            var declaredMessagePublisherFactory = _options.DeclaredMessagePublisherFactory ?? throw new ArgumentNullException();
            var declaredMessageSubscriberFactory = _options.DeclaredMessageSubscriberFactory ?? throw new ArgumentNullException();
            var contentExchangerFactory = _options.ContentExchangerFactory ?? throw new ArgumentNullException();
            var declaredMessageExchangerFactory = _options.DeclaredMessageExchangerFactory ?? throw new ArgumentNullException();
            var bytesPool = _options.BytesPool ?? throw new ArgumentNullException();

            var tcpConnectorOptions = OptionsGenerator.GenTcpConnectorOptions(config);
            var tcpConnector = await tcpConnectorFactory.CreateAsync(tcpConnectorOptions, socks5ProxyClientFactory, httpProxyClientFactory, upnpClientFactory, bytesPool);

            var ckadMediatorOptions = OptionsGenerator.GenCkadMediatorOptions(config);
            _ckadMediator = await ckadMediatorFactory.CreateAsync(ckadMediatorOptions, new[] { tcpConnector }, _options.BytesPool);

            var contentPublisherOptions = OptionsGenerator.GenContentPublisherOptions(config);
            _contentPublisher = await contentPublisherFactory.CreateAsync(contentPublisherOptions, bytesPool);

            var contentSubscriberOptions = OptionsGenerator.GenContentSubscriberOptions(config);
            _contentSubscriber = await contentSubscriberFactory.CreateAsync(contentSubscriberOptions, bytesPool);

            var declaredMessagePublisherOptions = OptionsGenerator.GenDeclaredMessagePublisherOptions(config);
            _declaredMessagePublisher = await declaredMessagePublisherFactory.CreateAsync(declaredMessagePublisherOptions, bytesPool);

            var declaredMessageSubscriberOptions = OptionsGenerator.GenDeclaredMessageSubscriberOptions(config);
            _declaredMessageSubscriber = await declaredMessageSubscriberFactory.CreateAsync(declaredMessageSubscriberOptions, bytesPool);

            var contentExchangerOptions = OptionsGenerator.GenContentExchangerOptions(config);
            _contentExchanger = await contentExchangerFactory.CreateAsync(contentExchangerOptions, new[] { tcpConnector }, _ckadMediator, _contentPublisher, _contentSubscriber, bytesPool);

            var declaredMessageExchangerOptions = OptionsGenerator.GenDeclaredMessageExchangerOptions(config);
            _declaredMessageExchanger = await declaredMessageExchangerFactory.CreateAsync(declaredMessageExchangerOptions, new[] { tcpConnector }, _ckadMediator, _declaredMessagePublisher, _declaredMessageSubscriber, bytesPool);
        }

        public async ValueTask<CkadMediator_GetMyNodeProfile_Output> CkadMediator_GetMyNodeProfileAsync(CancellationToken cancellationToken = default)
        {
            var result = await _ckadMediator.GetMyNodeProfileAsync(cancellationToken);
            return new CkadMediator_GetMyNodeProfile_Output(result);
        }

        public async ValueTask CkadMediator_AddCloudNodeProfilesAsync(CkadMediator_AddCloudNodeProfiles_Input param, CancellationToken cancellationToken = default)
        {
            await _ckadMediator.AddCloudNodeProfilesAsync(param.NodeProfiles, cancellationToken);
        }

        public ValueTask<ContentPublisher_GetReport_Output> ContentPublisher_GetReportAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<ContentPublisher_PublishContent_File_Output> ContentPublisher_PublishContentAsync(ContentPublisher_PublishContent_File_Input param, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<ContentPublisher_PublishContent_Memory_Output> ContentPublisher_PublishContentAsync(ContentPublisher_PublishContent_Memory_Input param, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask ContentPublisher_UnpublishContentAsync(ContentPublisher_UnpublishContent_File_Input param, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask ContentPublisher_UnpublishContentAsync(ContentPublisher_UnpublishContent_Memory_Input param, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<ContentSubscriber_GetReport_Output> ContentSubscriber_GetReportAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask ContentSubscriber_SubscribeContentAsync(ContentSubscriber_SubscribeContent_Input param, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask ContentSubscriber_UnsubscribeContentAsync(ContentSubscriber_UnsubscribeContent_Input param, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask ContentSubscriber_ExportContentAsync(ContentSubscriber_ExportContent_File_Input param, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<ContentSubscriber_ExportContent_Memory_Output> ContentSubscriber_ExportContentAsync(ContentSubscriber_ExportContent_Memory_Input param, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<DeclaredMessagePublisher_GetReport_Output> DeclaredMessagePublisher_GetReportAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask DeclaredMessagePublisher_PublishMessageAsync(DeclaredMessagePublisher_PublishMessage_Input param, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask DeclaredMessagePublisher_UnpublishMessageAsync(DeclaredMessagePublisher_UnpublishMessage_Input param, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<DeclaredMessageSubscriber_GetReport_Output> DeclaredMessageSubscriber_GetReportAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask DeclaredMessageSubscriber_SubscribeMessageAsync(DeclaredMessageSubscriber_SubscribeMessage_Input param, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask DeclaredMessageSubscriber_UnsubscribeMessageAsync(DeclaredMessageSubscriber_UnsubscribeMessage_Input param, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<DeclaredMessageSubscriber_ExportContent_Output> DeclaredMessageSubscriber_ExportContentAsync(DeclaredMessageSubscriber_ExportContent_Input param, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
