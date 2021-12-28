using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Omnius.Axis.Remoting;
using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Core.Net.Caps;
using Omnius.Core.Net.Connections.Bridge;
using Omnius.Core.Net.Connections.Multiplexer;
using Omnius.Core.RocketPack.Remoting;
using Omnius.Core.Tasks;
using MultiplexerV1 = Omnius.Core.Net.Connections.Multiplexer.V1;

namespace Omnius.Axis.Ui.Desktop.Internal;

internal sealed class ServiceManager : AsyncDisposableBase
{
    private readonly OmniAddress _listenAddress;

    private Process? _process;
    private Socket? _socket;
    private SocketCap? _cap;
    private BatchActionDispatcher? _batchActionDispatcher;
    private BridgeConnection? _bridgeConnection;
    private OmniConnectionMultiplexer? _multiplexer;
    private AxisServiceRemoting.Client<DefaultErrorMessage>? _axisServiceRemotingClient;

    public bool IsConnected => _multiplexer?.IsConnected ?? false;

    public static async ValueTask<ServiceManager> CreateAsync(OmniAddress listenAddress, CancellationToken cancellationToken = default)
    {
        var result = new ServiceManager(listenAddress);
        await result.InitAsync(cancellationToken);
        return result;
    }

    private ServiceManager(OmniAddress listenAddress)
    {
        _listenAddress = listenAddress;
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(_listenAddress);

        await this.ConnectAsync(cancellationToken);
    }

    private async ValueTask ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (!_listenAddress.TryGetTcpEndpoint(out var ipAddress, out var port)) throw new Exception("address is invalid format.");

        var bytesPool = BytesPool.Shared;

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

        _process?.Close();
        _process?.Dispose();
    }
}
