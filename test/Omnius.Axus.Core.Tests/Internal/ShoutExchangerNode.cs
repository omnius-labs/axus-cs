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

namespace Omnius.Axus.Core.Engine.Services;

internal class ShoutExchangerNode : AsyncDisposableBase
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
    private IShoutPublisherStorage _publishedShoutStorage = null!;
    private IShoutSubscriberStorage _subscribedShoutStorage = null!;
    private IShoutExchanger _shoutExchanger = null!;

    private AsyncLock _asyncLock = new();

    public static async ValueTask<ShoutExchangerNode> CreateAsync(OmniAddress listenAddress, CancellationToken cancellationToken = default)
    {
        var result = new ShoutExchangerNode(listenAddress);
        await result.InitAsync(cancellationToken);
        return result;
    }

    public ShoutExchangerNode(OmniAddress listenAddress)
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

        var publishedShoutStorageOptions = new ShoutPublisherStorageOptions
        {
            ConfigDirectoryPath = Path.Combine(_databaseDirectoryPath, "published_shout_storage")
        };
        _publishedShoutStorage = await ShoutPublisherStorage.CreateAsync(KeyValueRocksDbStorage.Factory, systemClock, bytesPool, publishedShoutStorageOptions, cancellationToken);

        var subscribedShoutStorageOptions = new ShoutSubscriberStorageOptions
        {
            ConfigDirectoryPath = Path.Combine(_databaseDirectoryPath, "subscribed_shout_storage")
        };
        _subscribedShoutStorage = await ShoutSubscriberStorage.CreateAsync(KeyValueRocksDbStorage.Factory, systemClock, bytesPool, subscribedShoutStorageOptions, cancellationToken);

        var shoutExchangerOptions = new ShoutExchangerOptions
        {
            MaxSessionCount = 128
        };
        _shoutExchanger = await ShoutExchanger.CreateAsync(_sessionConnector, _sessionAccepter, _nodeFinder, _publishedShoutStorage, _subscribedShoutStorage, systemClock, bytesPool, shoutExchangerOptions, cancellationToken);
    }

    protected override async ValueTask OnDisposeAsync()
    {
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

        foreach (var connectionAccepter in _connectionAcceptors)
        {
            await connectionAccepter.DisposeAsync();
        }

        _connectionAcceptors = _connectionAcceptors.Clear();

        _workingDirectoryRemover.Dispose();
    }

    public OmniAddress ListenAddress => _listenAddress;
    public INodeFinder GetNodeFinder() => _nodeFinder;
    public IShoutPublisherStorage GetPublishedShoutStorage() => _publishedShoutStorage;
    public IShoutSubscriberStorage GetSubscribedShoutStorage() => _subscribedShoutStorage;
}
