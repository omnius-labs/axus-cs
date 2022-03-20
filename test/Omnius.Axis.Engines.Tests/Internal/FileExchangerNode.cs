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

namespace Omnius.Axis.Engines;

internal class FileExchangerNode : AsyncDisposableBase
{
    private OmniAddress _listenAddress;
    private readonly IDisposable _workingDirectoryDeleter;
    private readonly string _databaseDirectoryPath;
    private readonly string _tempDirectoryPath;

    private ImmutableList<IConnectionConnector> _connectionConnectors = ImmutableList<IConnectionConnector>.Empty;
    private ImmutableList<IConnectionAccepter> _connectionAccepters = ImmutableList<IConnectionAccepter>.Empty;
    private ISessionConnector _sessionConnector = null!;
    private ISessionAccepter _sessionAccepter = null!;
    private INodeFinder _nodeFinder = null!;
    private IPublishedFileStorage _publishedFileStorage = null!;
    private ISubscribedFileStorage _subscribedFileStorage = null!;
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

        var connectionAccepters = ImmutableList.CreateBuilder<IConnectionAccepter>();

        {
            var tcpConnectionAccepterOption = new TcpConnectionAccepterOptions(false, _listenAddress);
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

        foreach (var connectionAccepter in _connectionAccepters)
        {
            await connectionAccepter.DisposeAsync();
        }

        _connectionAccepters = _connectionAccepters.Clear();
    }

    public OmniAddress ListenAddress => _listenAddress;

    public INodeFinder GetNodeFinder() => _nodeFinder;

    public IPublishedFileStorage GetPublishedFileStorage() => _publishedFileStorage;

    public ISubscribedFileStorage GetSubscribedFileStorage() => _subscribedFileStorage;
}
