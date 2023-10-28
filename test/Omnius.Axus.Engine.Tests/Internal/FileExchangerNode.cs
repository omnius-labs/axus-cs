using System.Collections.Immutable;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Proxies;
using Omnius.Core.Net.Upnp;
using Omnius.Core.Storages;
using Omnius.Core.UnitTestToolkit;

namespace Omnius.Axus.Engine.Internal.Services;

internal class FileExchangerNode : AsyncDisposableBase
{
    private OmniAddress _listenAddress;
    private readonly IDisposable _workingDirectoryRemover;
    private readonly string _databaseDirectoryPath;
    private readonly string _tempDirectoryPath;

    private ImmutableList<IConnectionConnector> _connectionConnectors = ImmutableList<IConnectionConnector>.Empty;
    private ImmutableList<IConnectionAcceptor> _connectionAcceptors = ImmutableList<IConnectionAcceptor>.Empty;
    private ISessionConnector _sessionConnector = null!;
    private ISessionAccepter _sessionAccepter = null!;
    private INodeFinder _nodeFinder = null!;
    private IFilePublisherStorage _publishedFileStorage = null!;
    private IFileSubscriberStorage _subscribedFileStorage = null!;
    private IFileExchanger _fileExchanger = null!;

    private AsyncLock _asyncLock = new();

    public static async ValueTask<FileExchangerNode> CreateAsync(OmniAddress listenAddress, CancellationToken cancellationToken = default)
    {
        var result = new FileExchangerNode(listenAddress);
        await result.InitAsync(cancellationToken);
        return result;
    }

    public FileExchangerNode(OmniAddress listenAddress)
    {
        _listenAddress = listenAddress;
        _workingDirectoryRemover = FixtureFactory.GenTempDirectory(out var workingDirectoryPath);
        _databaseDirectoryPath = Path.Combine(workingDirectoryPath, "db");
        _tempDirectoryPath = Path.Combine(workingDirectoryPath, "temp");

        DirectoryHelper.CreateDirectory(_databaseDirectoryPath);
        DirectoryHelper.CreateDirectory(_tempDirectoryPath);
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        var digitalSignature = OmniDigitalSignature.Create(Guid.NewGuid().ToString("N"), OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256);

        var bytesPool = BytesPool.Shared;
        var systemClock = SystemClock.Shared;
        var randomBytesProvider = RandomBytesProvider.Shared;

        var senderBandwidthLimiter = new BandwidthLimiter(int.MaxValue);
        var receiverBandwidthLimiter = new BandwidthLimiter(int.MaxValue);

        var nodeLocationsFetcherOptions = new NodeLocationsFetcherOptions
        {
            OperationType = NodeLocationsFetcherOperationType.None
        };
        var nodeLocationsFetcher = NodeLocationsFetcher.Create(nodeLocationsFetcherOptions);

        var connectionConnectors = ImmutableList.CreateBuilder<IConnectionConnector>();

        {
            var tcpConnectionConnectorOptions = new ConnectionTcpConnectorOptions
            {
                Proxy = new TcpProxyOptions
                {
                    Type = TcpProxyType.None,
                    Address = OmniAddress.Empty,
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

        {
            var tcpConnectionAccepterOption = new ConnectionTcpAccepterOptions
            {
                UseUpnp = false,
                ListenAddress = _listenAddress
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
        _nodeFinder = await NodeFinder.CreateAsync(_sessionConnector, _sessionAccepter, nodeLocationsFetcher, systemClock, bytesPool, nodeFinderOptions, cancellationToken);

        var publishedFileStorageOptions = new FilePublisherStorageOptions
        {
            ConfigDirectoryPath = Path.Combine(_databaseDirectoryPath, "published_file_storage")
        };
        _publishedFileStorage = await FilePublisherStorage.CreateAsync(KeyValueFileStorage.Factory, systemClock, randomBytesProvider, bytesPool, publishedFileStorageOptions, cancellationToken);

        var subscribedFileStorageOptions = new FileSubscriberStorageOptions
        {
            ConfigDirectoryPath = Path.Combine(_databaseDirectoryPath, "subscribed_file_storage")
        };
        _subscribedFileStorage = await FileSubscriberStorage.CreateAsync(KeyValueFileStorage.Factory, systemClock, randomBytesProvider, bytesPool, subscribedFileStorageOptions, cancellationToken);

        var fileExchangerOptions = new FileExchangerOptions
        {
            MaxSessionCount = 128
        };
        _fileExchanger = await FileExchanger.CreateAsync(_sessionConnector, _sessionAccepter, _nodeFinder, _publishedFileStorage, _subscribedFileStorage, systemClock, bytesPool, fileExchangerOptions, cancellationToken);
    }

    protected override async ValueTask OnDisposeAsync()
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

        _workingDirectoryRemover.Dispose();
    }

    public OmniAddress ListenAddress => _listenAddress;
    public INodeFinder GetNodeFinder() => _nodeFinder;
    public IFilePublisherStorage GetPublishedFileStorage() => _publishedFileStorage;
    public IFileSubscriberStorage GetSubscribedFileStorage() => _subscribedFileStorage;
}
