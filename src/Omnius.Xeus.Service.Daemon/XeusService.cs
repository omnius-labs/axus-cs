using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Proxies;
using Omnius.Core.Net.Upnp;
using Omnius.Core.Pipelines;
using Omnius.Core.Storages;
using Omnius.Core.Tasks;
using Omnius.Xeus.Service.Daemon.Configuration;
using Omnius.Xeus.Service.Engines;
using Omnius.Xeus.Service.Models;
using Omnius.Xeus.Service.Remoting;
using EnginesModels = Omnius.Xeus.Service.Engines;

namespace Omnius.Xeus.Service.Daemon
{
    public class XeusService : AsyncDisposableBase, IXeusService
    {
        private ISessionConnector _sessionConnector = null!;
        private ISessionAccepter _sessionAccepter = null!;
        private INodeFinder _nodeFinder = null!;
        private IPublishedFileStorage _publishedFileStorage = null!;
        private ISubscribedFileStorage _subscribedFileStorage = null!;
        private IFileExchanger _fileExchanger = null!;
        private IPublishedShoutStorage _publishedShoutStorage = null!;
        private ISubscribedShoutStorage _subscribedShoutStorage = null!;
        private IShoutExchanger _shoutExchanger = null!;

        private List<IDisposable> _disposables = new();

        public static async ValueTask<XeusService> CreateAsync(string workingDirectoryPath, AppConfig appConfig, CancellationToken cancellationToken = default)
        {
            var xeusService = new XeusService();
            await xeusService.InitAsync(workingDirectoryPath, appConfig, cancellationToken);
            return xeusService;
        }

        private XeusService()
        {
        }

        private async ValueTask InitAsync(string workingDirectoryPath, AppConfig appConfig, CancellationToken cancellationToken = default)
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
                var tcpConnectionConnector = await TcpConnectionConnector.CreateAsync(senderBandwidthLimiter, receiverBandwidthLimiter, Socks5ProxyClient.Factory, HttpProxyClient.Factory, batchActionDispatcher, bytesPool, tcpConnectionConnectorOptions, cancellationToken);
                connectionConnectors.Add(tcpConnectionConnector);
            }

            var sessionConnectorOptions = new SessionConnectorOptions(digitalSignature);
            _sessionConnector = await SessionConnector.CreateAsync(connectionConnectors, batchActionDispatcher, bytesPool, sessionConnectorOptions, cancellationToken);

            var connectionAccepters = new List<IConnectionAccepter>();

            foreach (var tcpConnectionAccepterConfig in appConfig.Engines?.SessionAccepter?.TcpAccepters ?? Array.Empty<TcpAccepterConfig>())
            {
                var useUpnp = tcpConnectionAccepterConfig.UseUpnp;
                var listenAddress = OmniAddress.Parse(tcpConnectionAccepterConfig.ListenAddress);
                var tcpConnectionAccepterOption = new TcpConnectionAccepterOptions(useUpnp, listenAddress);
                var tcpConnectionAccepter = await TcpConnectionAccepter.CreateAsync(senderBandwidthLimiter, receiverBandwidthLimiter, UpnpClient.Factory, batchActionDispatcher, bytesPool, tcpConnectionAccepterOption, cancellationToken);
                connectionAccepters.Add(tcpConnectionAccepter);
            }

            var sessionAccepterOptions = new SessionAccepterOptions(digitalSignature);
            _sessionAccepter = await SessionAccepter.CreateAsync(connectionAccepters, batchActionDispatcher, bytesPool, sessionAccepterOptions, cancellationToken);

            var nodeFinderOptions = new NodeFinderOptions(Path.Combine(workingDirectoryPath, "node_finder"), appConfig.Engines?.NodeFinder?.MaxSessionCount ?? int.MaxValue);
            _nodeFinder = await NodeFinder.CreateAsync(_sessionConnector, _sessionAccepter, batchActionDispatcher, bytesPool, nodeFinderOptions, cancellationToken);

            var publishedFileStorageOptions = new PublishedFileStorageOptions(Path.Combine(workingDirectoryPath, "published_file_storage"));
            _publishedFileStorage = await PublishedFileStorage.CreateAsync(KeyValueLiteDatabaseStorage.Factory, bytesPool, publishedFileStorageOptions, cancellationToken);

            var subscribedFileStorageOptions = new SubscribedFileStorageOptions(Path.Combine(workingDirectoryPath, "subscribed_file_storage"));
            _subscribedFileStorage = await SubscribedFileStorage.CreateAsync(KeyValueLiteDatabaseStorage.Factory, bytesPool, subscribedFileStorageOptions, cancellationToken);

            var fileExchangerOptions = new FileExchangerOptions(appConfig.Engines?.FileExchanger?.MaxSessionCount ?? int.MaxValue);
            _fileExchanger = await FileExchanger.CreateAsync(_sessionConnector, _sessionAccepter, _nodeFinder, _publishedFileStorage, _subscribedFileStorage, batchActionDispatcher, bytesPool, fileExchangerOptions, cancellationToken);
            _nodeFinder.GetEvents().GetContentExchangers.Subscribe(() => _fileExchanger).ToAdd(_disposables);

            var publishedShoutStorageOptions = new PublishedShoutStorageOptions(Path.Combine(workingDirectoryPath, "published_shout_storage"));
            _publishedShoutStorage = await PublishedShoutStorage.CreateAsync(KeyValueLiteDatabaseStorage.Factory, bytesPool, publishedShoutStorageOptions, cancellationToken);

            var subscribedShoutStorageOptions = new SubscribedShoutStorageOptions(Path.Combine(workingDirectoryPath, "subscribed_shout_storage"));
            _subscribedShoutStorage = await SubscribedShoutStorage.CreateAsync(KeyValueLiteDatabaseStorage.Factory, bytesPool, subscribedShoutStorageOptions, cancellationToken);

            var shoutExchangerOptions = new ShoutExchangerOptions(appConfig.Engines?.ShoutExchanger?.MaxSessionCount ?? int.MaxValue);
            _shoutExchanger = await ShoutExchanger.CreateAsync(_sessionConnector, _sessionAccepter, _nodeFinder, _publishedShoutStorage, _subscribedShoutStorage, batchActionDispatcher, bytesPool, shoutExchangerOptions, cancellationToken);
            _nodeFinder.GetEvents().GetContentExchangers.Subscribe(() => _shoutExchanger).ToAdd(_disposables);
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

            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }

        public async ValueTask<GetSessionsReportResult> GetSessionsReportAsync(CancellationToken cancellationToken = default)
        {
            var sessionReports = new List<SessionReport>();

            var nodeFinderReport = await _nodeFinder.GetReportAsync(cancellationToken);
            sessionReports.AddRange(nodeFinderReport.Sessions);

            var fileExchangerReport = await _fileExchanger.GetReportAsync(cancellationToken);
            sessionReports.AddRange(fileExchangerReport.Sessions);

            return new GetSessionsReportResult(sessionReports.ToArray());
        }

        public async ValueTask<GetPublishedFilesReportResult> GetPublishedFilesReportAsync(CancellationToken cancellationToken = default)
        {
            var publishedFileReports = new List<PublishedFileReport>();

            var publishedFileStorageReport = await _publishedFileStorage.GetReportAsync(cancellationToken);
            publishedFileReports.AddRange(publishedFileStorageReport.PublishedFiles);

            return new GetPublishedFilesReportResult(publishedFileReports.ToArray());
        }

        public async ValueTask<GetSubscribedFilesReportResult> GetSubscribedFilesReportAsync(CancellationToken cancellationToken = default)
        {
            var subscribedFileReports = new List<SubscribedFileReport>();

            var subscribedFileStorageReport = await _subscribedFileStorage.GetReportAsync(cancellationToken);
            subscribedFileReports.AddRange(subscribedFileStorageReport.SubscribedFiles);

            return new GetSubscribedFilesReportResult(subscribedFileReports.ToArray());
        }

        public async ValueTask<GetPublishedShoutsReportResult> GetPublishedShoutsReportAsync(CancellationToken cancellationToken = default)
        {
            var publishedShoutReports = new List<PublishedShoutReport>();

            var publishedShoutStorageReport = await _publishedShoutStorage.GetReportAsync(cancellationToken);
            publishedShoutReports.AddRange(publishedShoutStorageReport.PublishedShouts);

            return new GetPublishedShoutsReportResult(publishedShoutReports.ToArray());
        }

        public async ValueTask<GetSubscribedShoutsReportResult> GetSubscribedShoutsReportAsync(CancellationToken cancellationToken = default)
        {
            var subscribedShoutReports = new List<SubscribedShoutReport>();

            var subscribedShoutStorageReport = await _subscribedShoutStorage.GetReportAsync(cancellationToken);
            subscribedShoutReports.AddRange(subscribedShoutStorageReport.SubscribedShouts);

            return new GetSubscribedShoutsReportResult(subscribedShoutReports.ToArray());
        }

        public async ValueTask<GetMyNodeLocationResult> GetMyNodeLocationAsync(CancellationToken cancellationToken = default)
        {
            var myNodeLocation = await _nodeFinder.GetMyNodeLocationAsync(cancellationToken);
            return new GetMyNodeLocationResult(myNodeLocation);
        }

        public async ValueTask AddCloudNodeLocationsAsync(AddCloudNodeLocationsRequest param, CancellationToken cancellationToken = default)
        {
            await _nodeFinder.AddCloudNodeLocationsAsync(param.NodeLocations, cancellationToken);
        }

        public async ValueTask<PublishFileFromStorageResult> PublishFileFromStorageAsync(PublishFileFromStorageRequest param, CancellationToken cancellationToken = default)
        {
            var rootHash = await _publishedFileStorage.PublishFileAsync(param.FilePath, param.Registrant, cancellationToken);
            return new PublishFileFromStorageResult(rootHash);
        }

        public async ValueTask<PublishFileFromMemoryResult> PublishFileFromMemoryAsync(PublishFileFromMemoryRequest param, CancellationToken cancellationToken = default)
        {
            var rootHash = await _publishedFileStorage.PublishFileAsync(new ReadOnlySequence<byte>(param.Memory), param.Registrant, cancellationToken);
            return new PublishFileFromMemoryResult(rootHash);
        }

        public async ValueTask UnpublishFileFromStorageAsync(UnpublishFileFromStorageRequest param, CancellationToken cancellationToken = default)
        {
            await _publishedFileStorage.UnpublishFileAsync(param.FilePath, param.Registrant, cancellationToken);
        }

        public async ValueTask UnpublishFileFromMemoryAsync(UnpublishFileFromMemoryRequest param, CancellationToken cancellationToken = default)
        {
            await _publishedFileStorage.UnpublishFileAsync(param.RootHash, param.Registrant, cancellationToken);
        }

        public async ValueTask SubscribeFileAsync(SubscribeFileRequest param, CancellationToken cancellationToken = default)
        {
            await _subscribedFileStorage.SubscribeFileAsync(param.RootHash, param.Registrant, cancellationToken);
        }

        public async ValueTask UnsubscribeFileAsync(UnsubscribeFileRequest param, CancellationToken cancellationToken = default)
        {
            await _subscribedFileStorage.UnsubscribeFileAsync(param.RootHash, param.Registrant, cancellationToken);
        }

        public async ValueTask<TryExportFileToStorageResult> TryExportFileToStorageAsync(TryExportFileToStorageRequest param, CancellationToken cancellationToken = default)
        {
            var success = await _subscribedFileStorage.TryExportFileAsync(param.RootHash, param.FilePath, cancellationToken);
            return new TryExportFileToStorageResult(success);
        }

        public async ValueTask<TryExportFileToMemoryResult> TryExportFileToMemoryAsync(TryExportFileToMemoryRequest param, CancellationToken cancellationToken = default)
        {
            var bytesPool = BytesPool.Shared;
            using var bytesPipe = new BytesPipe(bytesPool);
            var success = await _subscribedFileStorage.TryExportFileAsync(param.RootHash, bytesPipe.Writer, cancellationToken);
            return new TryExportFileToMemoryResult(bytesPipe.Reader.GetSequence().ToMemory(bytesPool));
        }

        public async ValueTask PublishShoutAsync(PublishShoutRequest param, CancellationToken cancellationToken = default)
        {
            await _publishedShoutStorage.PublishShoutAsync(param.Shout, param.Registrant, cancellationToken);
        }

        public async ValueTask UnpublishShoutAsync(UnpublishShoutRequest param, CancellationToken cancellationToken = default)
        {
            await _publishedShoutStorage.UnpublishShoutAsync(param.Signature, param.Registrant, cancellationToken);
        }

        public async ValueTask SubscribeShoutAsync(SubscribeShoutRequest param, CancellationToken cancellationToken = default)
        {
            await _subscribedShoutStorage.SubscribeShoutAsync(param.Signature, param.Registrant, cancellationToken);
        }

        public async ValueTask UnsubscribeShoutAsync(UnsubscribeShoutRequest param, CancellationToken cancellationToken = default)
        {
            await _subscribedShoutStorage.UnsubscribeShoutAsync(param.Signature, param.Registrant, cancellationToken);
        }

        public async ValueTask<TryExportShoutResult> TryExportShoutAsync(TryExportShoutRequest param, CancellationToken cancellationToken = default)
        {
            var shout = await _subscribedShoutStorage.ReadShoutAsync(param.Signature, cancellationToken);
            return new TryExportShoutResult(shout);
        }
    }
}
