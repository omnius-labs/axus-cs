using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Net.Proxies;
using Omnius.Core.Net.Upnp;
using Omnius.Core.Pipelines;
using Omnius.Core.Storages;
using Omnius.Xeus.Engines.Connectors;
using Omnius.Xeus.Engines.Exchangers;
using Omnius.Xeus.Engines.Mediators;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines;

namespace Omnius.Xeus.Daemon
{
    public record XeusServiceOptions
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

        public IPublishedFileStorageFactory? PublishedFileStorageFactory { get; init; }

        public ISubscribedFileStorageFactory? SubscribedFileStorageFactory { get; init; }

        public IPublishedDeclaredMessageFactory? PublishedDeclaredMessageFactory { get; init; }

        public ISubscribedDeclaredMessageFactory? SubscribedDeclaredMessageFactory { get; init; }

        public TcpConnectionFactoryOptions? TcpConnectionFactoryOptions { get; init; }

        public CkadMediatorOptions? CkadMediatorOptions { get; init; }

        public PublishedFileStorageOptions? PublishedFileStorageOptions { get; init; }

        public SubscribedFileStorageOptions? SubscribedFileStorageOptions { get; init; }

        public PublishedDeclaredMessageOptions? PublishedDeclaredMessageOptions { get; init; }

        public SubscribedDeclaredMessageOptions? SubscribedDeclaredMessageOptions { get; init; }

        public ContentExchangerOptions? ContentExchangerOptions { get; init; }

        public DeclaredMessageExchangerOptions? DeclaredMessageExchangerOptions { get; init; }
    }

    public class XeusService : AsyncDisposableBase, IXeusService
    {
        private readonly XeusServiceOptions _options;

        private IBytesPool _bytesPool = null!;
        private IConnecitonMediator _ckadMediator = null!;
        private IContentExchanger _contentExchanger = null!;
        private IDeclaredMessageExchanger _declaredMessageExchanger = null!;
        private IPublishedFileStorage _contentPublisher = null!;
        private ISubscribedFileStorage _contentSubscriber = null!;
        private IPublishedShoutStorage _declaredMessagePublisher = null!;
        private ISubscribedShoutStorage _declaredMessageSubscriber = null!;

        public static async ValueTask<XeusService> CreateAsync(XeusServiceOptions options, CancellationToken cancellationToken = default)
        {
            var service = new XeusService(options);
            await service.InitAsync(cancellationToken);

            return service;
        }

        private XeusService(XeusServiceOptions options)
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
            var contentPublisherFactory = _options.PublishedFileStorageFactory ?? throw new ArgumentNullException();
            var contentSubscriberFactory = _options.SubscribedFileStorageFactory ?? throw new ArgumentNullException();
            var declaredMessagePublisherFactory = _options.PublishedDeclaredMessageFactory ?? throw new ArgumentNullException();
            var declaredMessageSubscriberFactory = _options.SubscribedDeclaredMessageFactory ?? throw new ArgumentNullException();
            var contentExchangerFactory = _options.ContentExchangerFactory ?? throw new ArgumentNullException();
            var declaredMessageExchangerFactory = _options.DeclaredMessageExchangerFactory ?? throw new ArgumentNullException();

            var tcpConnectionFactoryOptions = _options.TcpConnectionFactoryOptions ?? throw new ArgumentNullException();
            var ckadMediatorOptions = _options.CkadMediatorOptions ?? throw new ArgumentNullException();
            var contentPublisherOptions = _options.PublishedFileStorageOptions ?? throw new ArgumentNullException();
            var contentSubscriberOptions = _options.SubscribedFileStorageOptions ?? throw new ArgumentNullException();
            var declaredMessagePublisherOptions = _options.PublishedDeclaredMessageOptions ?? throw new ArgumentNullException();
            var declaredMessageSubscriberOptions = _options.SubscribedDeclaredMessageOptions ?? throw new ArgumentNullException();
            var contentExchangerOptions = _options.ContentExchangerOptions ?? throw new ArgumentNullException();
            var declaredMessageExchangerOptions = _options.DeclaredMessageExchangerOptions ?? throw new ArgumentNullException();

            var tcpConnectionFactory = await tcpConnectorFactory.CreateAsync(tcpConnectionFactoryOptions, socks5ProxyClientFactory, httpProxyClientFactory, upnpClientFactory, _bytesPool, cancellationToken);
            _ckadMediator = await ckadMediatorFactory.CreateAsync(ckadMediatorOptions, new[] { tcpConnectionFactory }, _bytesPool, cancellationToken);
            _contentPublisher = await contentPublisherFactory.CreateAsync(contentPublisherOptions, bytesStorageFactory, _bytesPool, cancellationToken);
            _contentSubscriber = await contentSubscriberFactory.CreateAsync(contentSubscriberOptions, bytesStorageFactory, _bytesPool, cancellationToken);
            _declaredMessagePublisher = await declaredMessagePublisherFactory.CreateAsync(declaredMessagePublisherOptions, bytesStorageFactory, _bytesPool, cancellationToken);
            _declaredMessageSubscriber = await declaredMessageSubscriberFactory.CreateAsync(declaredMessageSubscriberOptions, bytesStorageFactory, _bytesPool, cancellationToken);
            _contentExchanger = await contentExchangerFactory.CreateAsync(contentExchangerOptions, new[] { tcpConnectionFactory }, _ckadMediator, _contentPublisher, _contentSubscriber, _bytesPool, cancellationToken);
            _declaredMessageExchanger = await declaredMessageExchangerFactory.CreateAsync(declaredMessageExchangerOptions, new[] { tcpConnectionFactory }, _ckadMediator, _declaredMessagePublisher, _declaredMessageSubscriber, _bytesPool, cancellationToken);
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

        public async ValueTask<GetReportResult> GetReportAsync(CancellationToken cancellationToken = default)
        {
            return new GetReportResult();
        }

        public async ValueTask<GetMyNodeLocationResult> GetMyNodeLocationAsync(CancellationToken cancellationToken = default)
        {
            var result = await _ckadMediator.GetMyNodeLocationAsync(cancellationToken);
            return new GetMyNodeLocationResult(result);
        }

        public async ValueTask AddCloudNodeLocationsAsync(AddCloudNodeLocationsRequest request, CancellationToken cancellationToken = default)
        {
            await _ckadMediator.AddCloudNodeLocationsAsync(request.NodeLocations, cancellationToken);
        }

        public async ValueTask<PublishFileContentResult> PublishFileContentAsync(PublishFileContentRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _contentPublisher.PublishFileAsync(request.FilePath, request.Registrant, cancellationToken);
            return new PublishFileContentResult(result);
        }

        public async ValueTask<PublishMemoryContentResult> PublishMemoryContentAsync(PublishMemoryContentRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _contentPublisher.PublishFileAsync(new ReadOnlySequence<byte>(request.Memory), request.Registrant, cancellationToken);
            return new PublishMemoryContentResult(result);
        }

        public async ValueTask UnpublishFileContentAsync(UnpublishFileContentRequest request, CancellationToken cancellationToken = default)
        {
            await _contentPublisher.UnpublishFileAsync(request.FilePath, request.Registrant, cancellationToken);
        }

        public async ValueTask UnpublishMemoryContentAsync(UnpublishMemoryContentRequest request, CancellationToken cancellationToken = default)
        {
            await _contentPublisher.UnpublishFileAsync(request.RootHash, request.Registrant, cancellationToken);
        }

        public async ValueTask SubscribeContentAsync(SubscribeContentRequest request, CancellationToken cancellationToken = default)
        {
            await _contentSubscriber.SubscribeContentAsync(request.RootHash, request.Registrant, cancellationToken);
        }

        public async ValueTask UnsubscribeContentAsync(UnsubscribeContentRequest request, CancellationToken cancellationToken = default)
        {
            await _contentSubscriber.UnsubscribeContentAsync(request.RootHash, request.Registrant, cancellationToken);
        }

        public async ValueTask ExportFileContentAsync(ExportFileContentRequest request, CancellationToken cancellationToken = default)
        {
            await _contentSubscriber.ExportContentAsync(request.RootHash, request.FilePath, cancellationToken);
        }

        public async ValueTask<ExportMemoryContentResult> ExportMemoryContentAsync(ExportMemoryContentRequest request, CancellationToken cancellationToken = default)
        {
            using var hub = new BytesPipe();
            await _contentSubscriber.ExportContentAsync(request.RootHash, hub.Writer, cancellationToken);

            var sequence = hub.Reader.GetSequence();
            var memoryOwner = _bytesPool.Memory.Rent((int)sequence.Length).Shrink((int)sequence.Length);
            hub.Reader.GetSequence().CopyTo(memoryOwner.Memory.Span);
            return new ExportMemoryContentResult(memoryOwner);
        }

        public async ValueTask PublishDeclaredMessageAsync(PublishDeclaredMessageRequest request, CancellationToken cancellationToken = default)
        {
            await _declaredMessagePublisher.PublishShoutAsync(request.Message, request.Registrant, cancellationToken);
        }

        public async ValueTask UnpublishDeclaredMessageAsync(UnpublishDeclaredMessageRequest request, CancellationToken cancellationToken = default)
        {
            await _declaredMessagePublisher.UnpublishShoutAsync(request.Signature, request.Registrant, cancellationToken);
        }

        public async ValueTask SubscribeDeclaredMessageAsync(SubscribeDeclaredMessageRequest request, CancellationToken cancellationToken = default)
        {
            await _declaredMessageSubscriber.SubscribeMessageAsync(request.Signature, request.Registrant, cancellationToken);
        }

        public async ValueTask UnsubscribeDeclaredMessageAsync(UnsubscribeDeclaredMessageRequest request, CancellationToken cancellationToken = default)
        {
            await _declaredMessageSubscriber.UnsubscribeMessageAsync(request.Signature, request.Registrant, cancellationToken);
        }

        public async ValueTask<ExportDeclaredMessageResult> ExportDeclaredMessageAsync(ExportDeclaredMessageRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _declaredMessageSubscriber.ReadShoutAsync(request.Signature, cancellationToken);
            return new ExportDeclaredMessageResult(result);
        }
    }
}