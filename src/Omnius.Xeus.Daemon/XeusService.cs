using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Proxies;
using Omnius.Core.Net.Upnp;
using Omnius.Core.Storages;
using Omnius.Core.Tasks;
using Omnius.Xeus.Daemon.Configuration;
using Omnius.Xeus.Engines;
using Omnius.Xeus.Remoting;
using EnginesModels = Omnius.Xeus.Engines;

namespace Omnius.Xeus.Daemon
{
    public class XeusService : AsyncDisposableBase, IXeusService
    {
        private readonly ISessionConnector _sessionConnector;
        private readonly ISessionAccepter _sessionAccepter;
        private readonly INodeFinder _nodeFinder;
        private readonly IPublishedFileStorage _publishedFileStorage;
        private readonly ISubscribedFileStorage _subscribedFileStorage;
        private readonly IFileExchanger _fileExchanger;
        private readonly IPublishedShoutStorage _publishedShoutStorage;
        private readonly ISubscribedShoutStorage _subscribedShoutStorage;
        private readonly IShoutExchanger _shoutExchanger;

        public XeusService(string workingDirectoryPath, AppConfig appConfig)
        {
            var digitalSignature = OmniDigitalSignature.Create("", OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256);

            var bytesPool = BytesPool.Shared;
            var batchActionDispatcher = new BatchActionDispatcher(TimeSpan.FromMilliseconds(10));

            var senderBandwidthLimiter = new BandwidthLimiter(appConfig.Engines?.Bandwidth?.MaxSendBytesPerSeconds ?? int.MaxValue);
            var receiverBandwidthLimiter = new BandwidthLimiter(appConfig.Engines?.Bandwidth?.MaxReceiveBytesPerSeconds ?? int.MaxValue);

            var connectionConnectors = new List<IConnectionConnector>();

            foreach (var tcpConnectorConfig in appConfig.Engines?.SessionConnector?.TcpConnectors ?? Array.Empty<TcpConnectorConfig>())
            {
                var tcpProxyType = tcpConnectorConfig.Proxy?.Type switch
                {
                    Configuration.TcpProxyType.Unknown => EnginesModels.TcpProxyType.Unknown,
                    Configuration.TcpProxyType.HttpProxy => EnginesModels.TcpProxyType.HttpProxy,
                    Configuration.TcpProxyType.Socks5Proxy => EnginesModels.TcpProxyType.Socks5Proxy,
                    _ => EnginesModels.TcpProxyType.Unknown,
                };
                var tcpProxyAddress = OmniAddress.Parse(tcpConnectorConfig.Proxy?.Address);
                var tcpProxyOptions = new TcpProxyOptions(tcpProxyType, tcpProxyAddress);
                var tcpConnectionConnectorOptions = new TcpConnectionConnectorOptions(tcpProxyOptions);
                var tcpConnectionConnector = new TcpConnectionConnector(senderBandwidthLimiter, receiverBandwidthLimiter, Socks5ProxyClient.Factory, HttpProxyClient.Factory, batchActionDispatcher, bytesPool, tcpConnectionConnectorOptions);
                connectionConnectors.Add(tcpConnectionConnector);
            }

            var sessionConnectorOptions = new SessionConnectorOptions(digitalSignature);
            _sessionConnector = new SessionConnector(connectionConnectors, batchActionDispatcher, bytesPool, sessionConnectorOptions);

            var connectionAccepters = new List<IConnectionAccepter>();

            foreach (var tcpConnectionAccepterConfig in appConfig.Engines?.SessionAccepter?.TcpAccepters ?? Array.Empty<TcpAccepterConfig>())
            {
                var useUpnp = tcpConnectionAccepterConfig.UseUpnp;
                var listenAddress = OmniAddress.Parse(tcpConnectionAccepterConfig.ListenAddress);
                var tcpConnectionAccepterOption = new TcpConnectionAccepterOptions(useUpnp, listenAddress);
                var tcpConnectionAccepter = new TcpConnectionAccepter(senderBandwidthLimiter, receiverBandwidthLimiter, UpnpClient.Factory, batchActionDispatcher, bytesPool, tcpConnectionAccepterOption);
                connectionAccepters.Add(tcpConnectionAccepter);
            }

            var sessionAccepterOptions = new SessionAccepterOptions(digitalSignature);
            _sessionAccepter = new SessionAccepter(connectionAccepters, batchActionDispatcher, bytesPool, sessionAccepterOptions);

            var nodeFinderOptions = new NodeFinderOptions(workingDirectoryPath, appConfig.Engines?.NodeFinder?.MaxSessionCount ?? int.MaxValue);
            _nodeFinder = new NodeFinder(_sessionConnector, _sessionAccepter, bytesPool, nodeFinderOptions);

            var publishedFileStorageOptions = new PublishedFileStorageOptions(workingDirectoryPath);
            _publishedFileStorage = new PublishedFileStorage(LiteDatabaseBytesStorage.Factory, bytesPool, publishedFileStorageOptions);

            var subscribedFileStorageOptions = new SubscribedFileStorageOptions(workingDirectoryPath);
            _subscribedFileStorage = new SubscribedFileStorage(LiteDatabaseBytesStorage.Factory, bytesPool, subscribedFileStorageOptions);

            var fileExchangerOptions = new FileExchangerOptions(appConfig.Engines?.FileExchanger?.MaxSessionCount ?? int.MaxValue);
            _fileExchanger = new FileExchanger(_sessionConnector, _sessionAccepter, _nodeFinder, _publishedFileStorage, _subscribedFileStorage, bytesPool, fileExchangerOptions);

            var publishedShoutStorageOptions = new PublishedShoutStorageOptions(workingDirectoryPath);
            _publishedShoutStorage = new PublishedShoutStorage(LiteDatabaseBytesStorage.Factory, bytesPool, publishedShoutStorageOptions);

            var subscribedShoutStorageOptions = new SubscribedShoutStorageOptions(workingDirectoryPath);
            _subscribedShoutStorage = new SubscribedShoutStorage(LiteDatabaseBytesStorage.Factory, bytesPool, subscribedShoutStorageOptions);

            var shoutExchangerOptions = new ShoutExchangerOptions(appConfig.Engines?.ShoutExchanger?.MaxSessionCount ?? int.MaxValue);
            _shoutExchanger = new ShoutExchanger(_sessionConnector, _sessionAccepter, _nodeFinder, _publishedShoutStorage, _subscribedShoutStorage, bytesPool, shoutExchangerOptions);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            await _sessionConnector.DisposeAsync();
            await _sessionAccepter.DisposeAsync();
            await _nodeFinder.DisposeAsync();
            await _publishedFileStorage.DisposeAsync();
            await _subscribedFileStorage.DisposeAsync();
            await _fileExchanger.DisposeAsync();
            await _publishedShoutStorage.DisposeAsync();
            await _subscribedShoutStorage.DisposeAsync();
            await _shoutExchanger.DisposeAsync();
        }

        public ValueTask<GetSessionsReportResult> GetSessionsReportAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<GetPublishedFilesReportResult> GetPublishedFilesReportAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<GetSubscribedFilesReportResult> GetSubscribedFilesReportAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<GetPublishedShoutsReportResult> GetPublishedShoutsReportAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<GetSubscribedShoutsReportResult> GetSubscribedShoutsReportAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<GetMyNodeLocationResult> GetMyNodeLocationAsync(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask AddCloudNodeLocationsAsync(AddCloudNodeLocationsRequest param, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask<PublishFileFromStorageResult> PublishFileFromStorageAsync(PublishFileFromStorageRequest param, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask<PublishFileFromMemoryResult> PublishFileFromMemoryAsync(PublishFileFromMemoryRequest param, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask UnpublishFileFromStorageAsync(UnpublishFileFromStorageRequest param, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask UnpublishFileFromMemoryAsync(UnpublishFileFromMemoryRequest param, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask SubscribeFileAsync(SubscribeFileRequest param, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask UnsubscribeFileAsync(UnsubscribeFileRequest param, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask<TryExportFileToStorageResult> TryExportFileToStorageAsync(TryExportFileToStorageRequest param, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask<TryExportFileToMemoryResult> TryExportFileToMemoryAsync(TryExportFileToMemoryRequest param, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask PublishShoutAsync(PublishShoutRequest param, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask UnpublishShoutAsync(UnpublishShoutRequest param, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask SubscribeShoutAsync(SubscribeShoutRequest param, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask UnsubscribeShoutAsync(UnsubscribeShoutRequest param, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask<TryExportShoutResult> TryExportShoutAsync(TryExportShoutRequest param, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
