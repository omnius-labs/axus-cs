using System.Collections.Immutable;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Proxies;
using Omnius.Core.Net.Upnp;
using Omnius.Core.Storages;
using Omnius.Core.Tasks;
using Omnius.Core.UnitTestToolkit;

namespace Omnius.Axus.Engines.Internal;

internal class ShoutExchangerNode : AsyncDisposableBase
{
    private OmniAddress _listenAddress;
    private readonly IDisposable _workingDirectoryDeleter;
    private readonly string _databaseDirectoryPath;
    private readonly string _tempDirectoryPath;

    private ImmutableList<IConnectionConnector> _connectionConnectors = ImmutableList<IConnectionConnector>.Empty;
    private ImmutableList<IConnectionAccepter> _connectionAcceptors = ImmutableList<IConnectionAccepter>.Empty;
    private ISessionConnector _sessionConnector = null!;
    private ISessionAccepter _sessionAccepter = null!;
    private INodeFinder _nodeFinder = null!;
    private IPublishedShoutStorage _publishedShoutStorage = null!;
    private ISubscribedShoutStorage _subscribedShoutStorage = null!;
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
        _workingDirectoryDeleter = FixtureFactory.GenTempDirectory(out var workingDirectoryPath);
        _databaseDirectoryPath = Path.Combine(workingDirectoryPath, "db");
        _tempDirectoryPath = Path.Combine(workingDirectoryPath, "temp");

        DirectoryHelper.CreateDirectory(_databaseDirectoryPath);
        DirectoryHelper.CreateDirectory(_tempDirectoryPath);
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        var digitalSignature = OmniDigitalSignature.Create(Guid.NewGuid().ToString("N"), OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256);

        var bytesPool = BytesPool.Shared;
        var batchActionDispatcher = new BatchActionDispatcher(TimeSpan.FromMilliseconds(10));

        var senderBandwidthLimiter = new BandwidthLimiter(int.MaxValue);
        var receiverBandwidthLimiter = new BandwidthLimiter(int.MaxValue);

        var connectionConnectors = ImmutableList.CreateBuilder<IConnectionConnector>();

        {
            var tcpProxyType = Engines.TcpProxyType.None;
            var tcpProxyAddress = OmniAddress.Empty;
            var tcpProxyOptions = new TcpProxyOptions(tcpProxyType, tcpProxyAddress);
            var tcpConnectionConnectorOptions = new TcpConnectionConnectorOptions(tcpProxyOptions);
            var tcpConnectionConnector = await TcpConnectionConnector.CreateAsync(senderBandwidthLimiter, receiverBandwidthLimiter, Socks5ProxyClient.Factory, HttpProxyClient.Factory, batchActionDispatcher, bytesPool, tcpConnectionConnectorOptions, cancellationToken);
            connectionConnectors.Add(tcpConnectionConnector);
        }

        _connectionConnectors = connectionConnectors.ToImmutable();

        var sessionConnectorOptions = new SessionConnectorOptions(digitalSignature);
        _sessionConnector = await SessionConnector.CreateAsync(_connectionConnectors, batchActionDispatcher, bytesPool, sessionConnectorOptions, cancellationToken);

        var connectionAcceptors = ImmutableList.CreateBuilder<IConnectionAccepter>();

        {
            var tcpConnectionAccepterOption = new TcpConnectionAccepterOptions(false, _listenAddress);
            var tcpConnectionAccepter = await TcpConnectionAccepter.CreateAsync(senderBandwidthLimiter, receiverBandwidthLimiter, UpnpClient.Factory, batchActionDispatcher, bytesPool, tcpConnectionAccepterOption, cancellationToken);
            connectionAcceptors.Add(tcpConnectionAccepter);
        }

        _connectionAcceptors = connectionAcceptors.ToImmutable();

        var sessionAccepterOptions = new SessionAccepterOptions(digitalSignature);
        _sessionAccepter = await SessionAccepter.CreateAsync(_connectionAcceptors, batchActionDispatcher, bytesPool, sessionAccepterOptions, cancellationToken);

        var nodeFinderOptions = new NodeFinderOptions(Path.Combine(_databaseDirectoryPath, "node_finder"), 128);
        _nodeFinder = await NodeFinder.CreateAsync(_sessionConnector, _sessionAccepter, batchActionDispatcher, bytesPool, nodeFinderOptions, cancellationToken);

        var publishedShoutStorageOptions = new PublishedShoutStorageOptions(Path.Combine(_databaseDirectoryPath, "published_shout_storage"));
        _publishedShoutStorage = await PublishedShoutStorage.CreateAsync(KeyValueLiteDatabaseStorage.Factory, bytesPool, publishedShoutStorageOptions, cancellationToken);

        var subscribedShoutStorageOptions = new SubscribedShoutStorageOptions(Path.Combine(_databaseDirectoryPath, "subscribed_shout_storage"));
        _subscribedShoutStorage = await SubscribedShoutStorage.CreateAsync(KeyValueLiteDatabaseStorage.Factory, bytesPool, subscribedShoutStorageOptions, cancellationToken);

        var shoutExchangerOptions = new ShoutExchangerOptions(128);
        _shoutExchanger = await ShoutExchanger.CreateAsync(_sessionConnector, _sessionAccepter, _nodeFinder, _publishedShoutStorage, _subscribedShoutStorage, batchActionDispatcher, bytesPool, shoutExchangerOptions, cancellationToken);
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

        _workingDirectoryDeleter.Dispose();
    }

    public OmniAddress ListenAddress => _listenAddress;

    public INodeFinder GetNodeFinder() => _nodeFinder;

    public IPublishedShoutStorage GetPublishedShoutStorage() => _publishedShoutStorage;

    public ISubscribedShoutStorage GetSubscribedShoutStorage() => _subscribedShoutStorage;
}
