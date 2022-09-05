using System.Buffers;
using System.Collections.Immutable;
using System.Net;
using Omnius.Axus.Engines;
using Omnius.Axus.Models;
using Omnius.Axus.Remoting;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Proxies;
using Omnius.Core.Net.Upnp;
using Omnius.Core.Pipelines;
using Omnius.Core.Storages;
using Omnius.Core.Tasks;

namespace Omnius.Axus.Daemon;

public class AxusService : AsyncDisposableBase, IAxusService
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly string _databaseDirectoryPath;
    private readonly string _configPath;

    private ImmutableList<IConnectionConnector> _connectionConnectors = ImmutableList<IConnectionConnector>.Empty;
    private ImmutableList<IConnectionAccepter> _connectionAccepters = ImmutableList<IConnectionAccepter>.Empty;
    private ISessionConnector _sessionConnector = null!;
    private ISessionAccepter _sessionAccepter = null!;
    private INodeFinder _nodeFinder = null!;
    private IPublishedFileStorage _publishedFileStorage = null!;
    private ISubscribedFileStorage _subscribedFileStorage = null!;
    private IFileExchanger _fileExchanger = null!;
    private IPublishedShoutStorage _publishedShoutStorage = null!;
    private ISubscribedShoutStorage _subscribedShoutStorage = null!;
    private IShoutExchanger _shoutExchanger = null!;

    private AsyncLock _asyncLock = new();

    public static async ValueTask<AxusService> CreateAsync(string databaseDirectoryPath, CancellationToken cancellationToken = default)
    {
        var axusService = new AxusService(databaseDirectoryPath);
        await axusService.InitAsync(cancellationToken);
        return axusService;
    }

    private AxusService(string databaseDirectoryPath)
    {
        _databaseDirectoryPath = databaseDirectoryPath;
        DirectoryHelper.CreateDirectory(_databaseDirectoryPath);

        _configPath = Path.Combine(_databaseDirectoryPath, "config.yaml");
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        await this.StartAsync(cancellationToken);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        await this.StopAsync();
    }

    public async ValueTask<GetConfigResult> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var config = await this.InternalLoadConfigAsync(cancellationToken);
            return new GetConfigResult(config);
        }
    }

    public async ValueTask SetConfigAsync(SetConfigRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await this.InternalSaveConfigAsync(param.Config, cancellationToken);

            await this.StopAsync(cancellationToken);
            await this.StartAsync(cancellationToken);
        }
    }

    private async ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        var config = await this.InternalLoadConfigAsync(cancellationToken);

        var digitalSignature = OmniDigitalSignature.Create(Guid.NewGuid().ToString("N"), OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256);

        var bytesPool = BytesPool.Shared;
        var batchActionDispatcher = new BatchActionDispatcher(TimeSpan.FromMilliseconds(10));

        var senderBandwidthLimiter = new BandwidthLimiter(config.Bandwidth?.MaxSendBytesPerSeconds ?? int.MaxValue);
        var receiverBandwidthLimiter = new BandwidthLimiter(config.Bandwidth?.MaxReceiveBytesPerSeconds ?? int.MaxValue);

        var connectionConnectors = ImmutableList.CreateBuilder<IConnectionConnector>();

        if (config.I2pConnector is I2pConnectorConfig i2pConnectorConfig && i2pConnectorConfig.IsEnabled)
        {
            var i2pConnectionConnectorOptions = new I2pConnectionConnectorOptions(i2pConnectorConfig.SamBridgeAddress);
            var i2pConnectionConnector = await I2pConnectionConnector.CreateAsync(senderBandwidthLimiter, receiverBandwidthLimiter, batchActionDispatcher, bytesPool, i2pConnectionConnectorOptions, cancellationToken);
            connectionConnectors.Add(i2pConnectionConnector);
        }

        if (config.TcpConnector is TcpConnectorConfig tcpConnectorConfig && tcpConnectorConfig.IsEnabled)
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

        _connectionConnectors = connectionConnectors.ToImmutable();

        var sessionConnectorOptions = new SessionConnectorOptions(digitalSignature);
        _sessionConnector = await SessionConnector.CreateAsync(_connectionConnectors, batchActionDispatcher, bytesPool, sessionConnectorOptions, cancellationToken);

        var connectionAccepters = ImmutableList.CreateBuilder<IConnectionAccepter>();

        if (config.I2pAccepter is I2pAccepterConfig i2pAccepterConfig && i2pAccepterConfig.IsEnabled)
        {
            var i2pConnectionAccepterOptions = new I2pConnectionAccepterOptions(i2pAccepterConfig.SamBridgeAddress);
            var i2pConnectionAccepter = await I2pConnectionAccepter.CreateAsync(senderBandwidthLimiter, receiverBandwidthLimiter, batchActionDispatcher, bytesPool, i2pConnectionAccepterOptions, cancellationToken);
            connectionAccepters.Add(i2pConnectionAccepter);
        }

        if (config.TcpAccepter is TcpAccepterConfig tcpAccepterConfig && tcpAccepterConfig.IsEnabled)
        {
            var useUpnp = tcpAccepterConfig.UseUpnp;
            var listenAddress = tcpAccepterConfig.ListenAddress;
            var tcpConnectionAccepterOption = new TcpConnectionAccepterOptions(useUpnp, listenAddress);
            var tcpConnectionAccepter = await TcpConnectionAccepter.CreateAsync(senderBandwidthLimiter, receiverBandwidthLimiter, UpnpClient.Factory, batchActionDispatcher, bytesPool, tcpConnectionAccepterOption, cancellationToken);
            connectionAccepters.Add(tcpConnectionAccepter);
        }

        _connectionAccepters = connectionAccepters.ToImmutable();

        var sessionAccepterOptions = new SessionAccepterOptions(digitalSignature);
        _sessionAccepter = await SessionAccepter.CreateAsync(_connectionAccepters, batchActionDispatcher, bytesPool, sessionAccepterOptions, cancellationToken);

        var nodeFinderOptions = new NodeFinderOptions(Path.Combine(_databaseDirectoryPath, "node_finder"), 128);
        _nodeFinder = await NodeFinder.CreateAsync(_sessionConnector, _sessionAccepter, batchActionDispatcher, bytesPool, nodeFinderOptions, cancellationToken);

        var publishedFileStorageOptions = new PublishedFileStorageOptions(Path.Combine(_databaseDirectoryPath, "published_file_storage"));
        _publishedFileStorage = await PublishedFileStorage.CreateAsync(KeyValueLiteDatabaseStorage.Factory, bytesPool, publishedFileStorageOptions, cancellationToken);

        var subscribedFileStorageOptions = new SubscribedFileStorageOptions(Path.Combine(_databaseDirectoryPath, "subscribed_file_storage"));
        _subscribedFileStorage = await SubscribedFileStorage.CreateAsync(KeyValueLiteDatabaseStorage.Factory, bytesPool, subscribedFileStorageOptions, cancellationToken);

        var fileExchangerOptions = new FileExchangerOptions(128);
        _fileExchanger = await FileExchanger.CreateAsync(_sessionConnector, _sessionAccepter, _nodeFinder, _publishedFileStorage, _subscribedFileStorage, batchActionDispatcher, bytesPool, fileExchangerOptions, cancellationToken);

        var publishedShoutStorageOptions = new PublishedShoutStorageOptions(Path.Combine(_databaseDirectoryPath, "published_shout_storage"));
        _publishedShoutStorage = await PublishedShoutStorage.CreateAsync(KeyValueLiteDatabaseStorage.Factory, bytesPool, publishedShoutStorageOptions, cancellationToken);

        var subscribedShoutStorageOptions = new SubscribedShoutStorageOptions(Path.Combine(_databaseDirectoryPath, "subscribed_shout_storage"));
        _subscribedShoutStorage = await SubscribedShoutStorage.CreateAsync(KeyValueLiteDatabaseStorage.Factory, bytesPool, subscribedShoutStorageOptions, cancellationToken);

        var shoutExchangerOptions = new ShoutExchangerOptions(128);
        _shoutExchanger = await ShoutExchanger.CreateAsync(_sessionConnector, _sessionAccepter, _nodeFinder, _publishedShoutStorage, _subscribedShoutStorage, batchActionDispatcher, bytesPool, shoutExchangerOptions, cancellationToken);
    }

    private async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        if (_fileExchanger is not null)
        {
            await _fileExchanger.DisposeAsync();
            _fileExchanger = null!;
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

        if (_shoutExchanger is not null)
        {
            await _shoutExchanger.DisposeAsync();
            _shoutExchanger = null!;
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

        if (_nodeFinder is not null)
        {
            await _nodeFinder.DisposeAsync();
            _nodeFinder = null!;
        }

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

        foreach (var connectionConnector in _connectionConnectors)
        {
            await connectionConnector.DisposeAsync();
        }

        _connectionConnectors = _connectionConnectors.Clear();

        foreach (var connectionAccepter in _connectionAccepters)
        {
            await connectionAccepter.DisposeAsync();
        }

        _connectionAccepters = _connectionAccepters.Clear();
    }

    private async ValueTask<ServiceConfig> InternalLoadConfigAsync(CancellationToken cancellationToken = default)
    {
        var config = ServiceConfig.LoadFile(_configPath);

        if (config is null)
        {
            config = new ServiceConfig(
                bandwidth: new BandwidthConfig(
                    maxSendBytesPerSeconds: 1024 * 1024 * 32,
                    maxReceiveBytesPerSeconds: 1024 * 1024 * 32),
                i2pConnector: new I2pConnectorConfig(
                    isEnabled: true,
                    samBridgeAddress: OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 7656)),
                i2pAccepter: new I2pAccepterConfig(
                    isEnabled: true,
                    samBridgeAddress: OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 7656)),
                tcpConnector: new TcpConnectorConfig(
                    isEnabled: true,
                    proxy: null),
                tcpAccepter: new TcpAccepterConfig(
                    isEnabled: true,
                    useUpnp: true,
                    listenAddress: OmniAddress.CreateTcpEndpoint(IPAddress.Any, (ushort)Random.Shared.Next(10000, 60000))));

            config.SaveFile(_configPath);
        }

        return config;
    }

    private async ValueTask InternalSaveConfigAsync(ServiceConfig config, CancellationToken cancellationToken = default)
    {
        config.SaveFile(_configPath);
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

    public async ValueTask<GetCloudNodeLocationsResult> GetCloudNodeLocationsAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var cloudNodeLocations = await _nodeFinder.GetCloudNodeLocationsAsync(cancellationToken);
            return new GetCloudNodeLocationsResult(cloudNodeLocations.ToArray());
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
            var rootHash = await _publishedFileStorage.PublishFileAsync(param.FilePath, param.MaxBlockSize, param.Author, cancellationToken);
            return new PublishFileFromStorageResult(rootHash);
        }
    }

    public async ValueTask<PublishFileFromMemoryResult> PublishFileFromMemoryAsync(PublishFileFromMemoryRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var rootHash = await _publishedFileStorage.PublishFileAsync(new ReadOnlySequence<byte>(param.Memory), param.MaxBlockSize, param.Author, cancellationToken);
            return new PublishFileFromMemoryResult(rootHash);
        }
    }

    public async ValueTask UnpublishFileFromStorageAsync(UnpublishFileFromStorageRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _publishedFileStorage.UnpublishFileAsync(param.FilePath, param.Author, cancellationToken);
        }
    }

    public async ValueTask UnpublishFileFromMemoryAsync(UnpublishFileFromMemoryRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _publishedFileStorage.UnpublishFileAsync(param.RootHash, param.Author, cancellationToken);
        }
    }

    public async ValueTask SubscribeFileAsync(SubscribeFileRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _subscribedFileStorage.SubscribeFileAsync(param.RootHash, param.Author, cancellationToken);
        }
    }

    public async ValueTask UnsubscribeFileAsync(UnsubscribeFileRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _subscribedFileStorage.UnsubscribeFileAsync(param.RootHash, param.Author, cancellationToken);
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
            return new TryExportFileToMemoryResult(bytesPipe.Reader.GetSequence().ToMemoryOwner(bytesPool));
        }
    }

    public async ValueTask PublishShoutAsync(PublishShoutRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _publishedShoutStorage.PublishShoutAsync(param.Shout, param.Author, cancellationToken);
        }
    }

    public async ValueTask UnpublishShoutAsync(UnpublishShoutRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _publishedShoutStorage.UnpublishShoutAsync(param.Signature, param.Channel, param.Author, cancellationToken);
        }
    }

    public async ValueTask SubscribeShoutAsync(SubscribeShoutRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _subscribedShoutStorage.SubscribeShoutAsync(param.Signature, param.Channel, param.Author, cancellationToken);
        }
    }

    public async ValueTask UnsubscribeShoutAsync(UnsubscribeShoutRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _subscribedShoutStorage.UnsubscribeShoutAsync(param.Signature, param.Channel, param.Author, cancellationToken);
        }
    }

    public async ValueTask<TryExportShoutResult> TryExportShoutAsync(TryExportShoutRequest param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shout = await _subscribedShoutStorage.ReadShoutAsync(param.Signature, param.Channel, cancellationToken);
            return new TryExportShoutResult(shout);
        }
    }
}
