using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Extensions;
using Omnius.Core.Net.Proxies;
using Omnius.Core.Net.Upnp;
using Omnius.Core.Storages;
using Omnius.Xeus.Engines.Connectors;
using Omnius.Xeus.Engines.Exchangers;
using Omnius.Xeus.Engines.Mediators;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages;

namespace Omnius.Xeus.Daemon
{
    public class XeusServiceOptions
    {
        public IBytesPool? BytesPool { get; init; }

        public ISocks5ProxyClientFactory? Socks5ProxyClientFactory { get; init; }

        public IHttpProxyClientFactory? HttpProxyClientFactory { get; init; }

        public IUpnpClientFactory? UpnpClientFactory { get; init; }

        public IBytesStorageFactory? BytesStorageFactory { get; init; }

        public ITcpConnectorFactory? TcpConnectorFactory { get; init; }

        public ICkadMediatorFactory? CkadMediatorFactory { get; init; }

        public IContentExchangerFactory? ContentExchangerFactory { get; init; }

        public IDeclaredMessageExchangerFactory? DeclaredMessageExchangerFactory { get; init; }

        public IContentPublisherFactory? ContentPublisherFactory { get; init; }

        public IContentSubscriberFactory? ContentSubscriberFactory { get; init; }

        public IDeclaredMessagePublisherFactory? DeclaredMessagePublisherFactory { get; init; }

        public IDeclaredMessageSubscriberFactory? DeclaredMessageSubscriberFactory { get; init; }

        public TcpConnectorOptions? TcpConnectorOptions { get; init; }

        public CkadMediatorOptions? CkadMediatorOptions { get; init; }

        public ContentPublisherOptions? ContentPublisherOptions { get; init; }

        public ContentSubscriberOptions? ContentSubscriberOptions { get; init; }

        public DeclaredMessagePublisherOptions? DeclaredMessagePublisherOptions { get; init; }

        public DeclaredMessageSubscriberOptions? DeclaredMessageSubscriberOptions { get; init; }

        public ContentExchangerOptions? ContentExchangerOptions { get; init; }

        public DeclaredMessageExchangerOptions? DeclaredMessageExchangerOptions { get; init; }
    }

    public class XeusServiceImpl : AsyncDisposableBase, IXeusService
    {
        private readonly XeusServiceOptions _options;

        private IBytesPool _bytesPool = null!;
        private ICkadMediator _ckadMediator = null!;
        private IContentExchanger _contentExchanger = null!;
        private IDeclaredMessageExchanger _declaredMessageExchanger = null!;
        private IContentPublisher _contentPublisher = null!;
        private IContentSubscriber _contentSubscriber = null!;
        private IDeclaredMessagePublisher _declaredMessagePublisher = null!;
        private IDeclaredMessageSubscriber _declaredMessageSubscriber = null!;

        public static async ValueTask<XeusServiceImpl> CreateAsync(XeusServiceOptions options, CancellationToken cancellationToken = default)
        {
            var service = new XeusServiceImpl(options);
            await service.InitAsync(cancellationToken);

            return service;
        }

        private XeusServiceImpl(XeusServiceOptions options)
        {
            _options = options;
        }

        private async ValueTask InitAsync(CancellationToken cancellationToken = default)
        {
            _bytesPool = _options.BytesPool ?? throw new ArgumentNullException();

            var tcpConnectorFactory = _options.TcpConnectorFactory ?? throw new ArgumentNullException();
            var socks5ProxyClientFactory = _options.Socks5ProxyClientFactory ?? throw new ArgumentNullException();
            var httpProxyClientFactory = _options.HttpProxyClientFactory ?? throw new ArgumentNullException();
            var upnpClientFactory = _options.UpnpClientFactory ?? throw new ArgumentNullException();
            var bytesStorageFactory = _options.BytesStorageFactory ?? throw new ArgumentNullException();
            var ckadMediatorFactory = _options.CkadMediatorFactory ?? throw new ArgumentNullException();
            var contentPublisherFactory = _options.ContentPublisherFactory ?? throw new ArgumentNullException();
            var contentSubscriberFactory = _options.ContentSubscriberFactory ?? throw new ArgumentNullException();
            var declaredMessagePublisherFactory = _options.DeclaredMessagePublisherFactory ?? throw new ArgumentNullException();
            var declaredMessageSubscriberFactory = _options.DeclaredMessageSubscriberFactory ?? throw new ArgumentNullException();
            var contentExchangerFactory = _options.ContentExchangerFactory ?? throw new ArgumentNullException();
            var declaredMessageExchangerFactory = _options.DeclaredMessageExchangerFactory ?? throw new ArgumentNullException();

            var tcpConnectorOptions = _options.TcpConnectorOptions ?? throw new ArgumentNullException();
            var ckadMediatorOptions = _options.CkadMediatorOptions ?? throw new ArgumentNullException();
            var contentPublisherOptions = _options.ContentPublisherOptions ?? throw new ArgumentNullException();
            var contentSubscriberOptions = _options.ContentSubscriberOptions ?? throw new ArgumentNullException();
            var declaredMessagePublisherOptions = _options.DeclaredMessagePublisherOptions ?? throw new ArgumentNullException();
            var declaredMessageSubscriberOptions = _options.DeclaredMessageSubscriberOptions ?? throw new ArgumentNullException();
            var contentExchangerOptions = _options.ContentExchangerOptions ?? throw new ArgumentNullException();
            var declaredMessageExchangerOptions = _options.DeclaredMessageExchangerOptions ?? throw new ArgumentNullException();

            var tcpConnector = await tcpConnectorFactory.CreateAsync(tcpConnectorOptions, socks5ProxyClientFactory, httpProxyClientFactory, upnpClientFactory, _bytesPool, cancellationToken);
            _ckadMediator = await ckadMediatorFactory.CreateAsync(ckadMediatorOptions, new[] { tcpConnector }, _bytesPool, cancellationToken);
            _contentPublisher = await contentPublisherFactory.CreateAsync(contentPublisherOptions, bytesStorageFactory, _bytesPool, cancellationToken);
            _contentSubscriber = await contentSubscriberFactory.CreateAsync(contentSubscriberOptions, bytesStorageFactory, _bytesPool, cancellationToken);
            _declaredMessagePublisher = await declaredMessagePublisherFactory.CreateAsync(declaredMessagePublisherOptions, bytesStorageFactory, _bytesPool, cancellationToken);
            _declaredMessageSubscriber = await declaredMessageSubscriberFactory.CreateAsync(declaredMessageSubscriberOptions, bytesStorageFactory, _bytesPool, cancellationToken);
            _contentExchanger = await contentExchangerFactory.CreateAsync(contentExchangerOptions, new[] { tcpConnector }, _ckadMediator, _contentPublisher, _contentSubscriber, _bytesPool, cancellationToken);
            _declaredMessageExchanger = await declaredMessageExchangerFactory.CreateAsync(declaredMessageExchangerOptions, new[] { tcpConnector }, _ckadMediator, _declaredMessagePublisher, _declaredMessageSubscriber, _bytesPool, cancellationToken);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            await _declaredMessagePublisher.DisposeAsync();
            await _declaredMessageSubscriber.DisposeAsync();
            await _contentPublisher.DisposeAsync();
            await _contentSubscriber.DisposeAsync();
            await _contentExchanger.DisposeAsync();
            await _declaredMessageExchanger.DisposeAsync();
            await _ckadMediator.DisposeAsync();
        }

        public async ValueTask<CkadMediator_GetReport_Output> CkadMediator_GetReportAsync(CancellationToken cancellationToken = default)
        {
            var result = await _ckadMediator.GetReportAsync(cancellationToken);
            return new CkadMediator_GetReport_Output(result);
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

        public async ValueTask<ContentExchanger_GetReport_Output> ContentExchanger_GetReportAsync(CancellationToken cancellationToken = default)
        {
            var result = await _contentExchanger.GetReportAsync(cancellationToken);
            return new ContentExchanger_GetReport_Output(result);
        }

        public async ValueTask<DeclaredMessageExchanger_GetReport_Output> DeclaredMessageExchanger_GetReportAsync(CancellationToken cancellationToken = default)
        {
            var result = await _declaredMessageExchanger.GetReportAsync(cancellationToken);
            return new DeclaredMessageExchanger_GetReport_Output(result);
        }

        public async ValueTask<ContentPublisher_GetReport_Output> ContentPublisher_GetReportAsync(CancellationToken cancellationToken = default)
        {
            var result = await _contentPublisher.GetReportAsync(cancellationToken);
            return new ContentPublisher_GetReport_Output(result);
        }

        public async ValueTask<ContentPublisher_PublishContent_File_Output> ContentPublisher_PublishContentAsync(ContentPublisher_PublishContent_File_Input param, CancellationToken cancellationToken = default)
        {
            var result = await _contentPublisher.PublishContentAsync(param.FilePath, param.Registrant, cancellationToken);
            return new ContentPublisher_PublishContent_File_Output(result);
        }

        public async ValueTask<ContentPublisher_PublishContent_Memory_Output> ContentPublisher_PublishContentAsync(ContentPublisher_PublishContent_Memory_Input param, CancellationToken cancellationToken = default)
        {
            var result = await _contentPublisher.PublishContentAsync(new ReadOnlySequence<byte>(param.Memory), param.Registrant, cancellationToken);
            return new ContentPublisher_PublishContent_Memory_Output(result);
        }

        public async ValueTask ContentPublisher_UnpublishContentAsync(ContentPublisher_UnpublishContent_File_Input param, CancellationToken cancellationToken = default)
        {
            await _contentPublisher.UnpublishContentAsync(param.FilePath, param.Registrant, cancellationToken);
        }

        public async ValueTask ContentPublisher_UnpublishContentAsync(ContentPublisher_UnpublishContent_Memory_Input param, CancellationToken cancellationToken = default)
        {
            await _contentPublisher.UnpublishContentAsync(param.RootHash, param.Registrant, cancellationToken);
        }

        public async ValueTask<ContentSubscriber_GetReport_Output> ContentSubscriber_GetReportAsync(CancellationToken cancellationToken = default)
        {
            var result = await _contentSubscriber.GetReportAsync(cancellationToken);
            return new ContentSubscriber_GetReport_Output(result);
        }

        public async ValueTask ContentSubscriber_SubscribeContentAsync(ContentSubscriber_SubscribeContent_Input param, CancellationToken cancellationToken = default)
        {
            await _contentSubscriber.SubscribeContentAsync(param.RootHash, param.Registrant, cancellationToken);
        }

        public async ValueTask ContentSubscriber_UnsubscribeContentAsync(ContentSubscriber_UnsubscribeContent_Input param, CancellationToken cancellationToken = default)
        {
            await _contentSubscriber.UnsubscribeContentAsync(param.RootHash, param.Registrant, cancellationToken);
        }

        public async ValueTask ContentSubscriber_ExportContentAsync(ContentSubscriber_ExportContent_File_Input param, CancellationToken cancellationToken = default)
        {
            await _contentSubscriber.ExportContentAsync(param.RootHash, param.FilePath, cancellationToken);
        }

        public async ValueTask<ContentSubscriber_ExportContent_Memory_Output> ContentSubscriber_ExportContentAsync(ContentSubscriber_ExportContent_Memory_Input param, CancellationToken cancellationToken = default)
        {
            using var hub = new BytesHub();
            await _contentSubscriber.ExportContentAsync(param.RootHash, hub.Writer, cancellationToken);

            var sequence = hub.Reader.GetSequence();
            var memoryOwner = _bytesPool.Memory.Rent((int)sequence.Length).Shrink((int)sequence.Length);
            hub.Reader.GetSequence().CopyTo(memoryOwner.Memory.Span);
            return new ContentSubscriber_ExportContent_Memory_Output(memoryOwner);
        }

        public async ValueTask<DeclaredMessagePublisher_GetReport_Output> DeclaredMessagePublisher_GetReportAsync(CancellationToken cancellationToken = default)
        {
            var result = await _declaredMessagePublisher.GetReportAsync(cancellationToken);
            return new DeclaredMessagePublisher_GetReport_Output(result);
        }

        public async ValueTask DeclaredMessagePublisher_PublishMessageAsync(DeclaredMessagePublisher_PublishMessage_Input param, CancellationToken cancellationToken = default)
        {
            await _declaredMessagePublisher.PublishMessageAsync(param.Message, param.Registrant, cancellationToken);
        }

        public async ValueTask DeclaredMessagePublisher_UnpublishMessageAsync(DeclaredMessagePublisher_UnpublishMessage_Input param, CancellationToken cancellationToken = default)
        {
            await _declaredMessagePublisher.UnpublishMessageAsync(param.Signature, param.Registrant, cancellationToken);
        }

        public async ValueTask<DeclaredMessageSubscriber_GetReport_Output> DeclaredMessageSubscriber_GetReportAsync(CancellationToken cancellationToken = default)
        {
            var result = await _declaredMessageSubscriber.GetReportAsync(cancellationToken);
            return new DeclaredMessageSubscriber_GetReport_Output(result);
        }

        public async ValueTask DeclaredMessageSubscriber_SubscribeMessageAsync(DeclaredMessageSubscriber_SubscribeMessage_Input param, CancellationToken cancellationToken = default)
        {
            await _declaredMessageSubscriber.SubscribeMessageAsync(param.Signature, param.Registrant, cancellationToken);
        }

        public async ValueTask DeclaredMessageSubscriber_UnsubscribeMessageAsync(DeclaredMessageSubscriber_UnsubscribeMessage_Input param, CancellationToken cancellationToken = default)
        {
            await _declaredMessageSubscriber.UnsubscribeMessageAsync(param.Signature, param.Registrant, cancellationToken);
        }

        public async ValueTask<DeclaredMessageSubscriber_ExportMessage_Output> DeclaredMessageSubscriber_ExportMessageAsync(DeclaredMessageSubscriber_ExportMessage_Input param, CancellationToken cancellationToken = default)
        {
            var result = await _declaredMessageSubscriber.ReadMessageAsync(param.Signature, cancellationToken);
            return new DeclaredMessageSubscriber_ExportMessage_Output(result);
        }
    }
}
