using System.Buffers;
using System.Collections.Immutable;
using System.Net;
using Omnius.Axus.Engines;
using Omnius.Axus.Messages;
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

namespace Omnius.Axus.Daemon;

public class AxusService : AsyncDisposableBase, IAxusService
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly string _databaseDirectoryPath;
    private readonly string _configPath;

    private ImmutableList<IConnectionConnector> _connectionConnectors = ImmutableList<IConnectionConnector>.Empty;
    private ImmutableList<IConnectionAcceptor> _connectionAcceptors = ImmutableList<IConnectionAcceptor>.Empty;
    private ISessionConnector _sessionConnector = null!;
    private ISessionAccepter _sessionAccepter = null!;
    private INodeFinder _nodeFinder = null!;
    private IFilePublisherStorage _filePublisherStorage = null!;
    private IFileSubscriberStorage _fileSubscriberStorage = null!;
    private IFileExchanger _fileExchanger = null!;
    private IShoutPublisherStorage _shoutPublisherStorage = null!;
    private IShoutSubscriberStorage _shoutSubscriberStorage = null!;
    private IShoutExchanger _shoutExchanger = null!;

    private AsyncLock _asyncLock = new();

    private const string NODES_URI = "http://app.omnius-labs.com/axus/v1/nodes.txt";

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

    public async ValueTask SetConfigAsync(SetConfigParam param, CancellationToken cancellationToken = default)
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

        var senderBandwidthLimiter = new BandwidthLimiter(config.Bandwidth?.MaxSendBytesPerSeconds ?? int.MaxValue);
        var receiverBandwidthLimiter = new BandwidthLimiter(config.Bandwidth?.MaxReceiveBytesPerSeconds ?? int.MaxValue);

        var nodeLocationsFetcherOptions = new NodeLocationsFetcherOptions
        {
            Uri = NODES_URI,
            OperationType = NodeLocationsFetcherOperationType.HttpGet
        };
        var nodeLocationsFetcher = NodeLocationsFetcher.Create(nodeLocationsFetcherOptions);

        var connectionConnectors = ImmutableList.CreateBuilder<IConnectionConnector>();

        if (config.I2pConnector is I2pConnectorConfig i2pConnectorConfig && i2pConnectorConfig.IsEnabled)
        {
            var i2pConnectionConnectorOptions = new ConnectionI2pConnectorOptions
            {
                SamBridgeAddress = i2pConnectorConfig.SamBridgeAddress
            };
            var i2pConnectionConnector = await ConnectionI2pConnector.CreateAsync(senderBandwidthLimiter, receiverBandwidthLimiter, bytesPool, i2pConnectionConnectorOptions, cancellationToken);
            connectionConnectors.Add(i2pConnectionConnector);
        }

        if (config.TcpConnector is TcpConnectorConfig tcpConnectorConfig && tcpConnectorConfig.IsEnabled)
        {
            var tcpConnectionConnectorOptions = new ConnectionTcpConnectorOptions
            {
                Proxy = new TcpProxyOptions
                {
                    Type = tcpConnectorConfig.Proxy?.Type switch
                    {
                        Remoting.TcpProxyType.None => Engines.TcpProxyType.None,
                        Remoting.TcpProxyType.HttpProxy => Engines.TcpProxyType.HttpProxy,
                        Remoting.TcpProxyType.Socks5Proxy => Engines.TcpProxyType.Socks5Proxy,
                        _ => Engines.TcpProxyType.None,
                    },
                    Address = tcpConnectorConfig.Proxy?.Address ?? OmniAddress.Empty
                }
            };
            var tcpConnectionConnector = await ConnectionTcpConnector.CreateAsync(senderBandwidthLimiter, receiverBandwidthLimiter, Socks5ProxyClient.Factory, HttpProxyClient.Factory, bytesPool, tcpConnectionConnectorOptions, cancellationToken);
            connectionConnectors.Add(tcpConnectionConnector);
        }

        _connectionConnectors = connectionConnectors.ToImmutable();

        var sessionConnectorOptions = new SessionConnectorOptions
        {
            DigitalSignature = digitalSignature
        };
        _sessionConnector = await SessionConnector.CreateAsync(_connectionConnectors, bytesPool, sessionConnectorOptions, cancellationToken);

        var connectionAcceptors = ImmutableList.CreateBuilder<IConnectionAcceptor>();

        if (config.I2pAccepter is I2pAccepterConfig i2pAccepterConfig && i2pAccepterConfig.IsEnabled)
        {
            var i2pConnectionAccepterOptions = new ConnectionI2pAccepterOptions
            {
                SamBridgeAddress = i2pAccepterConfig.SamBridgeAddress
            };
            var i2pConnectionAccepter = await ConnectionI2pAccepter.CreateAsync(senderBandwidthLimiter, receiverBandwidthLimiter, bytesPool, i2pConnectionAccepterOptions, cancellationToken);
            connectionAcceptors.Add(i2pConnectionAccepter);
        }

        if (config.TcpAccepter is TcpAccepterConfig tcpAccepterConfig && tcpAccepterConfig.IsEnabled)
        {
            var tcpConnectionAccepterOption = new ConnectionTcpAccepterOptions
            {
                UseUpnp = tcpAccepterConfig.UseUpnp,
                ListenAddress = tcpAccepterConfig.ListenAddress
            };
            var tcpConnectionAccepter = await ConnectionTcpAccepter.CreateAsync(senderBandwidthLimiter, receiverBandwidthLimiter, UpnpClient.Factory, bytesPool, tcpConnectionAccepterOption, cancellationToken);
            connectionAcceptors.Add(tcpConnectionAccepter);
        }

        _connectionAcceptors = connectionAcceptors.ToImmutable();

        var sessionAccepterOptions = new SessionAccepterOptions
        {
            DigitalSignature = digitalSignature
        };
        _sessionAccepter = await SessionAccepter.CreateAsync(_connectionAcceptors, bytesPool, sessionAccepterOptions, cancellationToken);

        var nodeFinderOptions = new NodeFinderOptions
        {
            ConfigDirectoryPath = Path.Combine(_databaseDirectoryPath, "node_finder"),
            MaxSessionCount = 128
        };
        _nodeFinder = await NodeFinder.CreateAsync(_sessionConnector, _sessionAccepter, nodeLocationsFetcher, bytesPool, nodeFinderOptions, cancellationToken);

        var filePublisherStorageOptions = new FilePublisherStorageOptions
        {
            ConfigDirectoryPath = Path.Combine(_databaseDirectoryPath, "file_publisher_storage")
        };
        _filePublisherStorage = await FilePublisherStorage.CreateAsync(KeyValueFileStorage.Factory, bytesPool, filePublisherStorageOptions, cancellationToken);

        var fileSubscriberStorageOptions = new FileSubscriberStorageOptions
        {
            ConfigDirectoryPath = Path.Combine(_databaseDirectoryPath, "file_subscriber_storage")
        };
        _fileSubscriberStorage = await FileSubscriberStorage.CreateAsync(KeyValueFileStorage.Factory, bytesPool, fileSubscriberStorageOptions, cancellationToken);

        var fileExchangerOptions = new FileExchangerOptions
        {
            MaxSessionCount = 128
        };
        _fileExchanger = await FileExchanger.CreateAsync(_sessionConnector, _sessionAccepter, _nodeFinder, _filePublisherStorage, _fileSubscriberStorage, bytesPool, fileExchangerOptions, cancellationToken);

        var shoutPublisherStorageOptions = new ShoutPublisherStorageOptions
        {
            ConfigDirectoryPath = Path.Combine(_databaseDirectoryPath, "shout_publisher_storage")
        };
        _shoutPublisherStorage = await ShoutPublisherStorage.CreateAsync(KeyValueFileStorage.Factory, bytesPool, shoutPublisherStorageOptions, cancellationToken);

        var shoutSubscriberStorageOptions = new ShoutSubscriberStorageOptions
        {
            ConfigDirectoryPath = Path.Combine(_databaseDirectoryPath, "shout_subscriber_storage")
        };
        _shoutSubscriberStorage = await ShoutSubscriberStorage.CreateAsync(KeyValueFileStorage.Factory, bytesPool, shoutSubscriberStorageOptions, cancellationToken);

        var shoutExchangerOptions = new ShoutExchangerOptions
        {
            MaxSessionCount = 128
        };
        _shoutExchanger = await ShoutExchanger.CreateAsync(_sessionConnector, _sessionAccepter, _nodeFinder, _shoutPublisherStorage, _shoutSubscriberStorage, bytesPool, shoutExchangerOptions, cancellationToken);
    }

    private async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        if (_fileExchanger is not null)
        {
            await _fileExchanger.DisposeAsync();
            _fileExchanger = null!;
        }

        if (_filePublisherStorage is not null)
        {
            await _filePublisherStorage.DisposeAsync();
            _filePublisherStorage = null!;
        }

        if (_fileSubscriberStorage is not null)
        {
            await _fileSubscriberStorage.DisposeAsync();
            _fileSubscriberStorage = null!;
        }

        if (_shoutExchanger is not null)
        {
            await _shoutExchanger.DisposeAsync();
            _shoutExchanger = null!;
        }

        if (_shoutPublisherStorage is not null)
        {
            await _shoutPublisherStorage.DisposeAsync();
            _shoutPublisherStorage = null!;
        }

        if (_shoutSubscriberStorage is not null)
        {
            await _shoutSubscriberStorage.DisposeAsync();
            _shoutSubscriberStorage = null!;
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

        foreach (var connectionAccepter in _connectionAcceptors)
        {
            await connectionAccepter.DisposeAsync();
        }

        _connectionAcceptors = _connectionAcceptors.Clear();
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

    public async ValueTask<GetPublishedFilesReportResult> GetPublishedFilesReportAsync(GetPublishedFilesReportParam param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _filePublisherStorage.GetPublishedFileReportsAsync(param.Zone, cancellationToken);
            return new GetPublishedFilesReportResult(reports.ToArray());
        }
    }

    public async ValueTask<GetSubscribedFilesReportResult> GetSubscribedFilesReportAsync(GetSubscribedFilesReportParam param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _fileSubscriberStorage.GetSubscribedFileReportsAsync(param.Zone, cancellationToken);
            return new GetSubscribedFilesReportResult(reports.ToArray());
        }
    }

    public async ValueTask<GetPublishedShoutsReportResult> GetPublishedShoutsReportAsync(GetPublishedShoutsReportParam param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _shoutPublisherStorage.GetPublishedShoutReportsAsync(param.Zone, cancellationToken);
            return new GetPublishedShoutsReportResult(reports.ToArray());
        }
    }

    public async ValueTask<GetSubscribedShoutsReportResult> GetSubscribedShoutsReportAsync(GetSubscribedShoutsReportParam param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var reports = await _shoutSubscriberStorage.GetSubscribedShoutReportsAsync(param.Zone, cancellationToken);
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

    public async ValueTask AddCloudNodeLocationsAsync(AddCloudNodeLocationsParam param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _nodeFinder.AddCloudNodeLocationsAsync(param.NodeLocations, cancellationToken);
        }
    }

    public async ValueTask<PublishFileFromStorageResult> PublishFileFromStorageAsync(PublishFileFromStorageParam param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var rootHash = await _filePublisherStorage.PublishFileAsync(param.FilePath, param.MaxBlockSize, param.Properties, param.Zone, cancellationToken);
            return new PublishFileFromStorageResult(rootHash);
        }
    }

    public async ValueTask<PublishFileFromMemoryResult> PublishFileFromMemoryAsync(PublishFileFromMemoryParam param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var rootHash = await _filePublisherStorage.PublishFileAsync(new ReadOnlySequence<byte>(param.Memory), param.MaxBlockSize, param.Properties, param.Zone, cancellationToken);
            return new PublishFileFromMemoryResult(rootHash);
        }
    }

    public async ValueTask UnpublishFileFromStorageAsync(UnpublishFileFromStorageParam param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _filePublisherStorage.UnpublishFileAsync(param.FilePath, param.Zone, cancellationToken);
        }
    }

    public async ValueTask UnpublishFileFromMemoryAsync(UnpublishFileFromMemoryParam param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _filePublisherStorage.UnpublishFileAsync(param.RootHash, param.Zone, cancellationToken);
        }
    }

    public async ValueTask SubscribeFileAsync(SubscribeFileParam param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _fileSubscriberStorage.SubscribeFileAsync(param.RootHash, param.Zone, param.Properties, cancellationToken);
        }
    }

    public async ValueTask UnsubscribeFileAsync(UnsubscribeFileParam param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _fileSubscriberStorage.UnsubscribeFileAsync(param.RootHash, param.Zone, cancellationToken);
        }
    }

    public async ValueTask<TryExportFileToStorageResult> TryExportFileToStorageAsync(TryExportFileToStorageParam param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var success = await _fileSubscriberStorage.TryExportFileAsync(param.RootHash, param.FilePath, cancellationToken);
            return new TryExportFileToStorageResult(success);
        }
    }

    public async ValueTask<TryExportFileToMemoryResult> TryExportFileToMemoryAsync(TryExportFileToMemoryParam param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var bytesPool = BytesPool.Shared;
            using var bytesPipe = new BytesPipe(bytesPool);
            var success = await _fileSubscriberStorage.TryExportFileAsync(param.RootHash, bytesPipe.Writer, cancellationToken);
            return new TryExportFileToMemoryResult(bytesPipe.Reader.GetSequence().ToMemoryOwner(bytesPool));
        }
    }

    public async ValueTask PublishShoutAsync(PublishShoutParam param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _shoutPublisherStorage.PublishShoutAsync(param.Shout, param.Properties, param.Zone, cancellationToken);
        }
    }

    public async ValueTask UnpublishShoutAsync(UnpublishShoutParam param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _shoutPublisherStorage.UnpublishShoutAsync(param.Signature, param.Channel, param.Zone, cancellationToken);
        }
    }

    public async ValueTask SubscribeShoutAsync(SubscribeShoutParam param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _shoutSubscriberStorage.SubscribeShoutAsync(param.Signature, param.Properties, param.Channel, param.Zone, cancellationToken);
        }
    }

    public async ValueTask UnsubscribeShoutAsync(UnsubscribeShoutParam param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await _shoutSubscriberStorage.UnsubscribeShoutAsync(param.Signature, param.Channel, param.Zone, cancellationToken);
        }
    }

    public async ValueTask<TryExportShoutResult> TryExportShoutAsync(TryExportShoutParam param, CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var shout = await _shoutSubscriberStorage.TryReadShoutAsync(param.Signature, param.Channel, param.CreatedTime.ToDateTime(), cancellationToken);
            return new TryExportShoutResult(shout);
        }
    }
}
