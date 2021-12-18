using System.Net;
using System.Net.Sockets;
using Omnius.Axis.Intaractors;
using Omnius.Axis.Remoting;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Core.Net.Caps;
using Omnius.Core.Net.Connections.Bridge;
using Omnius.Core.Net.Connections.Multiplexer;
using Omnius.Core.RocketPack.Remoting;
using Omnius.Core.Storages;
using Omnius.Core.Tasks;
using MultiplexerV1 = Omnius.Core.Net.Connections.Multiplexer.V1;

namespace Omnius.Axis.Ui.Desktop;

public interface IIntaractorProvider
{
    ValueTask<Dashboard> GetDashboardAsync(CancellationToken cancellationToken = default);

    ValueTask<FileDownloader> GetFileDownloaderAsync(CancellationToken cancellationToken = default);

    ValueTask<FileUploader> GetFileUploaderAsync(CancellationToken cancellationToken = default);

    ValueTask<ProfilePublisher> GetProfilePublisherAsync(CancellationToken cancellationToken = default);

    ValueTask<ProfileSubscriber> GetProfileSubscriberAsync(CancellationToken cancellationToken = default);
}

public class IntaractorProvider : AsyncDisposableBase, IIntaractorProvider
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly AppConfig _appConfig;
    private readonly IBytesPool _bytesPool;

    private ServiceManager? _serviceManager;

    private Dashboard? _dashboard;
    private FileDownloader? _fileDownloader;
    private FileUploader? _fileUploader;
    private ProfilePublisher? _profilePublisher;
    private ProfileSubscriber? _profileSubscriber;

    private readonly AsyncLock _asyncLock = new();

    public IntaractorProvider(AppConfig appConfig, IBytesPool bytesPool)
    {
        _appConfig = appConfig;
        _bytesPool = bytesPool;
    }

    protected override async ValueTask OnDisposeAsync()
    {
        if (_serviceManager is not null) await _serviceManager.DisposeAsync();
    }

    public async ValueTask<Dashboard> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await this.UpdateAsync(cancellationToken);
            return _dashboard!;
        }
    }

    public async ValueTask<FileDownloader> GetFileDownloaderAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await this.UpdateAsync(cancellationToken);
            return _fileDownloader!;
        }
    }

    public async ValueTask<FileUploader> GetFileUploaderAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await this.UpdateAsync(cancellationToken);
            return _fileUploader!;
        }
    }

    public async ValueTask<ProfilePublisher> GetProfilePublisherAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await this.UpdateAsync(cancellationToken);
            return _profilePublisher!;
        }
    }

    public async ValueTask<ProfileSubscriber> GetProfileSubscriberAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            await this.UpdateAsync(cancellationToken);
            return _profileSubscriber!;
        }
    }

    private async ValueTask UpdateAsync(CancellationToken cancellationToken = default)
    {
        for (; ; )
        {
            if (_serviceManager is not null && _serviceManager.IsConnected)
            {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await this.StopAsync(cancellationToken);
                await this.StartAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.Warn(e);
            }
        }
    }

    private async ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        _serviceManager = new ServiceManager();
        await _serviceManager.ConnectAsync(OmniAddress.Parse(_appConfig.DaemonAddress), _bytesPool, cancellationToken);
        var service = _serviceManager.GetService()!;

        _dashboard = await Dashboard.CreateAsync(service, _bytesPool, cancellationToken);

        var fileUploaderOptions = new FileUploaderOptions(Path.Combine(_appConfig.DatabaseDirectoryPath!, "file_uploader"));
        _fileUploader = await FileUploader.CreateAsync(service, KeyValueLiteDatabaseStorage.Factory, _bytesPool, fileUploaderOptions, cancellationToken);

        var fileDownloaderOptions = new FileDownloaderOptions(Path.Combine(_appConfig.DatabaseDirectoryPath!, "file_downloader"));
        _fileDownloader = await FileDownloader.CreateAsync(service, SingleValueFileStorage.Factory, KeyValueLiteDatabaseStorage.Factory, _bytesPool, fileDownloaderOptions, cancellationToken);

        var profilePublisherOptions = new ProfilePublisherOptions(Path.Combine(_appConfig.DatabaseDirectoryPath!, "profile_publisher"));
        _profilePublisher = await ProfilePublisher.CreateAsync(service, KeyValueLiteDatabaseStorage.Factory, _bytesPool, profilePublisherOptions);

        var profileSubscriberOptions = new ProfileSubscriberOptions(Path.Combine(_appConfig.DatabaseDirectoryPath!, "profile_subscriber"));
        _profileSubscriber = await ProfileSubscriber.CreateAsync(service, SingleValueFileStorage.Factory, KeyValueLiteDatabaseStorage.Factory, _bytesPool, profileSubscriberOptions);
    }

    private async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        if (_dashboard is not null) await _dashboard.DisposeAsync();
        if (_fileUploader is not null) await _fileUploader.DisposeAsync();
        if (_fileDownloader is not null) await _fileDownloader.DisposeAsync();
        if (_profilePublisher is not null) await _profilePublisher.DisposeAsync();
        if (_profileSubscriber is not null) await _profileSubscriber.DisposeAsync();
        if (_serviceManager is not null) await _serviceManager.DisposeAsync();
    }

    private class ServiceManager : AsyncDisposableBase
    {
        private Socket? _socket;
        private SocketCap? _cap;
        private BatchActionDispatcher? _batchActionDispatcher;
        private BridgeConnection? _bridgeConnection;
        private OmniConnectionMultiplexer? _multiplexer;
        private AxisServiceRemoting.Client<DefaultErrorMessage>? _axisServiceRemotingClient;

        public bool IsConnected => _multiplexer?.IsConnected ?? false;

        public async ValueTask ConnectAsync(OmniAddress address, IBytesPool bytesPool, CancellationToken cancellationToken = default)
        {
            if (!address.TryGetTcpEndpoint(out var ipAddress, out var port)) throw new Exception("address is invalid format.");

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await _socket.ConnectAsync(new IPEndPoint(ipAddress, port), TimeSpan.FromSeconds(3), cancellationToken);

            _cap = new SocketCap(_socket);

            _batchActionDispatcher = new BatchActionDispatcher(TimeSpan.FromMilliseconds(10));

            var bridgeConnectionOptions = new BridgeConnectionOptions(int.MaxValue);
            _bridgeConnection = new BridgeConnection(_cap, null, null, _batchActionDispatcher, bytesPool, bridgeConnectionOptions);

            var multiplexerOptions = new MultiplexerV1.OmniConnectionMultiplexerOptions(OmniConnectionMultiplexerType.Connected, TimeSpan.FromMilliseconds(1000 * 10), 10, uint.MaxValue, 10);
            _multiplexer = OmniConnectionMultiplexer.CreateV1(_bridgeConnection, _batchActionDispatcher, bytesPool, multiplexerOptions);

            await _multiplexer.HandshakeAsync(cancellationToken);

            var rocketRemotingCallerFactory = new RocketRemotingCallerFactory<DefaultErrorMessage>(_multiplexer, bytesPool);
            _axisServiceRemotingClient = new AxisServiceRemoting.Client<DefaultErrorMessage>(rocketRemotingCallerFactory, bytesPool);
        }

        public IAxisService? GetService() => _axisServiceRemotingClient;

        protected override async ValueTask OnDisposeAsync()
        {
            if (_multiplexer is not null) await _multiplexer.DisposeAsync();
            if (_bridgeConnection is not null) await _bridgeConnection.DisposeAsync();
            if (_batchActionDispatcher is not null) await _batchActionDispatcher.DisposeAsync();
            _cap?.Dispose();
            _socket?.Dispose();
        }
    }
}
