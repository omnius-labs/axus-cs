using System.Buffers;
using System.Net;
using Omnius.Axis.Engines;
using Omnius.Axis.Models;
using Omnius.Axis.Remoting;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Proxies;
using Omnius.Core.Net.Upnp;
using Omnius.Core.Pipelines;
using Omnius.Core.Storages;
using Omnius.Core.Tasks;

namespace Omnius.Axis.Daemon;

public class AxisService : AsyncDisposableBase, IAxisService
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly string _databaseDirectoryPath;
    private ISingleValueStorage _configStorage = null!;

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

    private AsyncLock _asyncLock = new();

    public static async ValueTask<AxisService> CreateAsync(string databaseDirectoryPath, CancellationToken cancellationToken = default)
    {
        var axisService = new AxisService(databaseDirectoryPath);
        await axisService.InitAsync(cancellationToken);
        return axisService;
    }

    private AxisService(string databaseDirectoryPath)
    {
        _databaseDirectoryPath = databaseDirectoryPath;
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        _configStorage = SingleValueFileStorage.Factory.Create(Path.Combine(_databaseDirectoryPath, "config"), BytesPool.Shared);

        await this.StartAsync(cancellationToken);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        await this.StopAsync();

        _configStorage.Dispose();
    }

    public async ValueTask<GetConfigResult> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var config = await this.LoadConfigAsync(cancellationToken);
            return new GetConfigResult(config);
        }
    }

    public async ValueTask SetConfigAsync(SetConfigRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await this.SaveConfigAsync(param.Config, cancellationToken);

            await this.StopAsync(cancellationToken);
            await this.StartAsync(cancellationToken);
        }
    }

    private async ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        var config = await this.LoadConfigAsync(cancellationToken);

        var digitalSignature = OmniDigitalSignature.Create(Guid.NewGuid().ToString("N"), OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256);

        var bytesPool = BytesPool.Shared;
        var batchActionDispatcher = new BatchActionDispatcher(TimeSpan.FromMilliseconds(10));

        var senderBandwidthLimiter = new BandwidthLimiter(config.Bandwidth?.MaxSendBytesPerSeconds ?? int.MaxValue);
        var receiverBandwidthLimiter = new BandwidthLimiter(config.Bandwidth?.MaxReceiveBytesPerSeconds ?? int.MaxValue);

        var connectionConnectors = new List<IConnectionConnector>();

        if (config.TcpConnector is TcpConnectorConfig tcpConnectorConfig)
        {
            var tcpProxyType = tcpConnectorConfig.Proxy?.Type switch
            {
                Models.TcpProxyType.None => Engines.TcpProxyType.None,
                Models.TcpProxyType.HttpProxy => Engines.TcpProxyType.HttpProxy,
                Models.TcpProxyType.Socks5Proxy => Engines.TcpProxyType.Socks5Proxy,
                _ => Engines.TcpProxyType.None,
            };
            var tcpProxyAddress = tcpConnectorConfig.Proxy?.Address ?? OmniAddress.Empty;
            var tcpProxyOptions = new TcpProxyOptions(tcpProxyType, tcpProxyAddress);
            var tcpConnectionConnectorOptions = new TcpConnectionConnectorOptions(tcpProxyOptions);
            var tcpConnectionConnector = await TcpConnectionConnector.CreateAsync(senderBandwidthLimiter, receiverBandwidthLimiter, Socks5ProxyClient.Factory, HttpProxyClient.Factory, batchActionDispatcher, bytesPool, tcpConnectionConnectorOptions, cancellationToken);
            connectionConnectors.Add(tcpConnectionConnector);
        }

        var sessionConnectorOptions = new SessionConnectorOptions(digitalSignature);
        _sessionConnector = await SessionConnector.CreateAsync(connectionConnectors, batchActionDispatcher, bytesPool, sessionConnectorOptions, cancellationToken);

        var connectionAccepters = new List<IConnectionAccepter>();

        if (config.TcpAccepter is TcpAccepterConfig tcpAccepterConfig)
        {
            var useUpnp = tcpAccepterConfig.UseUpnp;
            var listenAddress = tcpAccepterConfig.ListenAddress;
            var tcpConnectionAccepterOption = new TcpConnectionAccepterOptions(useUpnp, listenAddress);
            var tcpConnectionAccepter = await TcpConnectionAccepter.CreateAsync(senderBandwidthLimiter, receiverBandwidthLimiter, UpnpClient.Factory, batchActionDispatcher, bytesPool, tcpConnectionAccepterOption, cancellationToken);
            connectionAccepters.Add(tcpConnectionAccepter);
        }

        var sessionAccepterOptions = new SessionAccepterOptions(digitalSignature);
        _sessionAccepter = await SessionAccepter.CreateAsync(connectionAccepters, batchActionDispatcher, bytesPool, sessionAccepterOptions, cancellationToken);

        var nodeFinderOptions = new NodeFinderOptions(Path.Combine(_databaseDirectoryPath, "node_finder"), 128);
        _nodeFinder = await NodeFinder.CreateAsync(_sessionConnector, _sessionAccepter, batchActionDispatcher, bytesPool, nodeFinderOptions, cancellationToken);

        var publishedFileStorageOptions = new PublishedFileStorageOptions(Path.Combine(_databaseDirectoryPath, "published_file_storage"));
        _publishedFileStorage = await PublishedFileStorage.CreateAsync(KeyValueLiteDatabaseStorage.Factory, bytesPool, publishedFileStorageOptions, cancellationToken);

        var subscribedFileStorageOptions = new SubscribedFileStorageOptions(Path.Combine(_databaseDirectoryPath, "subscribed_file_storage"));
        _subscribedFileStorage = await SubscribedFileStorage.CreateAsync(KeyValueLiteDatabaseStorage.Factory, bytesPool, subscribedFileStorageOptions, cancellationToken);

        var fileExchangerOptions = new FileExchangerOptions(128);
        _fileExchanger = await FileExchanger.CreateAsync(_sessionConnector, _sessionAccepter, _nodeFinder, _publishedFileStorage, _subscribedFileStorage, batchActionDispatcher, bytesPool, fileExchangerOptions, cancellationToken);
        _nodeFinder.GetEvents().GetContentExchangers.Subscribe(() => _fileExchanger).ToAdd(_disposables);

        var publishedShoutStorageOptions = new PublishedShoutStorageOptions(Path.Combine(_databaseDirectoryPath, "published_shout_storage"));
        _publishedShoutStorage = await PublishedShoutStorage.CreateAsync(KeyValueLiteDatabaseStorage.Factory, bytesPool, publishedShoutStorageOptions, cancellationToken);

        var subscribedShoutStorageOptions = new SubscribedShoutStorageOptions(Path.Combine(_databaseDirectoryPath, "subscribed_shout_storage"));
        _subscribedShoutStorage = await SubscribedShoutStorage.CreateAsync(KeyValueLiteDatabaseStorage.Factory, bytesPool, subscribedShoutStorageOptions, cancellationToken);

        var shoutExchangerOptions = new ShoutExchangerOptions(128);
        _shoutExchanger = await ShoutExchanger.CreateAsync(_sessionConnector, _sessionAccepter, _nodeFinder, _publishedShoutStorage, _subscribedShoutStorage, batchActionDispatcher, bytesPool, shoutExchangerOptions, cancellationToken);
        _nodeFinder.GetEvents().GetContentExchangers.Subscribe(() => _shoutExchanger).ToAdd(_disposables);
    }

    private async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        if (_sessionConnector is not null)
        {
            await _sessionConnector.DisposeAsync();
            _sessionConnector = null!;
        }

        if (_sessionAccepter is not null)
        {
            await _sessionAccepter.DisposeAsync();
            _sessionAccepter = null!;
        }

        if (_nodeFinder is not null)
        {
            await _nodeFinder.DisposeAsync();
            _nodeFinder = null!;
        }

        if (_publishedFileStorage is not null)
        {
            await _publishedFileStorage.DisposeAsync();
            _publishedFileStorage = null!;
        }

        if (_subscribedFileStorage is not null)
        {
            await _subscribedFileStorage.DisposeAsync();
            _subscribedFileStorage = null!;
        }

        if (_fileExchanger is not null)
        {
            await _fileExchanger.DisposeAsync();
            _fileExchanger = null!;
        }

        if (_publishedShoutStorage is not null)
        {
            await _publishedShoutStorage.DisposeAsync();
            _publishedShoutStorage = null!;
        }

        if (_subscribedShoutStorage is not null)
        {
            await _subscribedShoutStorage.DisposeAsync();
            _subscribedShoutStorage = null!;
        }

        if (_shoutExchanger is not null)
        {
            await _shoutExchanger.DisposeAsync();
            _shoutExchanger = null!;
        }

        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }

        _disposables.Clear();
    }

    private async ValueTask<ServiceConfig> LoadConfigAsync(CancellationToken cancellationToken = default)
    {
        var config = await _configStorage.TryGetValueAsync<ServiceConfig>(cancellationToken);

        if (config is null)
        {
            config = new ServiceConfig(
                bandwidth: new BandwidthConfig(
                    maxSendBytesPerSeconds: 1024 * 1024 * 32,
                    maxReceiveBytesPerSeconds: 1024 * 1024 * 32),
                tcpConnector: new TcpConnectorConfig(
                    proxy: null),
                tcpAccepter: new TcpAccepterConfig(
                    useUpnp: true,
                    listenAddress: OmniAddress.CreateTcpEndpoint(IPAddress.Any, (ushort)Random.Shared.Next(10000, 60000))));

            await _configStorage.TrySetValueAsync(config, cancellationToken);
        }

        return config;
    }

    private async ValueTask SaveConfigAsync(ServiceConfig config, CancellationToken cancellationToken = default)
    {
        await _configStorage.TrySetValueAsync(config, cancellationToken);
    }

    public async ValueTask<GetSessionsReportResult> GetSessionsReportAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = new List<SessionReport>();
            reports.AddRange(await _nodeFinder.GetSessionReportsAsync(cancellationToken));
            reports.AddRange(await _fileExchanger.GetSessionReportsAsync(cancellationToken));

            return new GetSessionsReportResult(reports.ToArray());
        }
    }

    public async ValueTask<GetPublishedFilesReportResult> GetPublishedFilesReportAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _publishedFileStorage.GetPublishedFileReportsAsync(cancellationToken);
            return new GetPublishedFilesReportResult(reports.ToArray());
        }
    }

    public async ValueTask<GetSubscribedFilesReportResult> GetSubscribedFilesReportAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _subscribedFileStorage.GetSubscribedFileReportsAsync(cancellationToken);
            return new GetSubscribedFilesReportResult(reports.ToArray());
        }
    }

    public async ValueTask<GetPublishedShoutsReportResult> GetPublishedShoutsReportAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _publishedShoutStorage.GetPublishedShoutReportsAsync(cancellationToken);
            return new GetPublishedShoutsReportResult(reports.ToArray());
        }
    }

    public async ValueTask<GetSubscribedShoutsReportResult> GetSubscribedShoutsReportAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _subscribedShoutStorage.GetSubscribedShoutReportsAsync(cancellationToken);
            return new GetSubscribedShoutsReportResult(reports.ToArray());
        }
    }

    public async ValueTask<GetMyNodeLocationResult> GetMyNodeLocationAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var myNodeLocation = await _nodeFinder.GetMyNodeLocationAsync(cancellationToken);
            return new GetMyNodeLocationResult(myNodeLocation);
        }
    }

    public async ValueTask AddCloudNodeLocationsAsync(AddCloudNodeLocationsRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _nodeFinder.AddCloudNodeLocationsAsync(param.NodeLocations, cancellationToken);
        }
    }

    public async ValueTask<PublishFileFromStorageResult> PublishFileFromStorageAsync(PublishFileFromStorageRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var rootHash = await _publishedFileStorage.PublishFileAsync(param.FilePath, param.Registrant, cancellationToken);
            return new PublishFileFromStorageResult(rootHash);
        }
    }

    public async ValueTask<PublishFileFromMemoryResult> PublishFileFromMemoryAsync(PublishFileFromMemoryRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var rootHash = await _publishedFileStorage.PublishFileAsync(new ReadOnlySequence<byte>(param.Memory), param.Registrant, cancellationToken);
            return new PublishFileFromMemoryResult(rootHash);
        }
    }

    public async ValueTask UnpublishFileFromStorageAsync(UnpublishFileFromStorageRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _publishedFileStorage.UnpublishFileAsync(param.FilePath, param.Registrant, cancellationToken);
        }
    }

    public async ValueTask UnpublishFileFromMemoryAsync(UnpublishFileFromMemoryRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _publishedFileStorage.UnpublishFileAsync(param.RootHash, param.Registrant, cancellationToken);
        }
    }

    public async ValueTask SubscribeFileAsync(SubscribeFileRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _subscribedFileStorage.SubscribeFileAsync(param.RootHash, param.Registrant, cancellationToken);
        }
    }

    public async ValueTask UnsubscribeFileAsync(UnsubscribeFileRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _subscribedFileStorage.UnsubscribeFileAsync(param.RootHash, param.Registrant, cancellationToken);
        }
    }

    public async ValueTask<TryExportFileToStorageResult> TryExportFileToStorageAsync(TryExportFileToStorageRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var success = await _subscribedFileStorage.TryExportFileAsync(param.RootHash, param.FilePath, cancellationToken);
            return new TryExportFileToStorageResult(success);
        }
    }

    public async ValueTask<TryExportFileToMemoryResult> TryExportFileToMemoryAsync(TryExportFileToMemoryRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var bytesPool = BytesPool.Shared;
            using var bytesPipe = new BytesPipe(bytesPool);
            var success = await _subscribedFileStorage.TryExportFileAsync(param.RootHash, bytesPipe.Writer, cancellationToken);
            return new TryExportFileToMemoryResult(bytesPipe.Reader.GetSequence().ToMemory(bytesPool));
        }
    }

    public async ValueTask PublishShoutAsync(PublishShoutRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _publishedShoutStorage.PublishShoutAsync(param.Shout, param.Registrant, cancellationToken);
        }
    }

    public async ValueTask UnpublishShoutAsync(UnpublishShoutRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _publishedShoutStorage.UnpublishShoutAsync(param.Signature, param.Registrant, cancellationToken);
        }
    }

    public async ValueTask SubscribeShoutAsync(SubscribeShoutRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _subscribedShoutStorage.SubscribeShoutAsync(param.Signature, param.Registrant, cancellationToken);
        }
    }

    public async ValueTask UnsubscribeShoutAsync(UnsubscribeShoutRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _subscribedShoutStorage.UnsubscribeShoutAsync(param.Signature, param.Registrant, cancellationToken);
        }
    }

    public async ValueTask<TryExportShoutResult> TryExportShoutAsync(TryExportShoutRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shout = await _subscribedShoutStorage.ReadShoutAsync(param.Signature, cancellationToken);
            return new TryExportShoutResult(shout);
        }
    }
}
