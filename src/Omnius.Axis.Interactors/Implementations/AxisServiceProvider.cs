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

namespace Omnius.Axis.Interactors;

public sealed class AxisServiceProvider : AsyncDisposableBase, IAxisServiceProvider
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly OmniAddress _listenAddress;

    private Socket? _socket;
    private SocketCap? _cap;
    private BatchActionDispatcher? _batchActionDispatcher;
    private BridgeConnection? _bridgeConnection;
    private OmniConnectionMultiplexer? _multiplexer;
    private AxisServiceRemoting.Client<DefaultErrorMessage>? _axisServiceRemotingClient;

    public bool IsConnected => _multiplexer?.IsConnected ?? false;

    public static async ValueTask<AxisServiceProvider> CreateAsync(OmniAddress listenAddress, CancellationToken cancellationToken = default)
    {
        var result = new AxisServiceProvider(listenAddress);
        await result.InitAsync(cancellationToken);
        return result;
    }

    private AxisServiceProvider(OmniAddress listenAddress)
    {
        _listenAddress = listenAddress;
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(_listenAddress);

        using var timeoutCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellationToken.CancelAfter(TimeSpan.FromSeconds(60));

        await this.ConnectAsync(cancellationToken);
    }

    private async ValueTask ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (!_listenAddress.TryParseTcpEndpoint(out var ipAddress, out var port)) throw new Exception("address is invalid format.");

        var bytesPool = BytesPool.Shared;

        _socket = await ConnectSocketAsync(ipAddress, port, cancellationToken);
        _cap = new SocketCap(_socket);

        _batchActionDispatcher = new BatchActionDispatcher(TimeSpan.FromMilliseconds(10));

        var bridgeConnectionOptions = new BridgeConnectionOptions(int.MaxValue);
        _bridgeConnection = new BridgeConnection(_cap, null, null, _batchActionDispatcher, bytesPool, bridgeConnectionOptions);

        var multiplexerOptions = new MultiplexerV1.OmniConnectionMultiplexerOptions(OmniConnectionMultiplexerType.Connected, TimeSpan.FromMinutes(1), 10, int.MaxValue, 10);
        _multiplexer = OmniConnectionMultiplexer.CreateV1(_bridgeConnection, _batchActionDispatcher, bytesPool, multiplexerOptions);

        await _multiplexer.HandshakeAsync(cancellationToken);

        var rocketRemotingCallerFactory = new RocketRemotingCallerFactory<DefaultErrorMessage>(_multiplexer, bytesPool);
        _axisServiceRemotingClient = new AxisServiceRemoting.Client<DefaultErrorMessage>(rocketRemotingCallerFactory, bytesPool);
    }

    private static async ValueTask<Socket> ConnectSocketAsync(IPAddress ipAddress, ushort port, CancellationToken cancellationToken = default)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        for (; ; )
        {
            try
            {
                await socket.ConnectAsync(new IPEndPoint(ipAddress, port), TimeSpan.FromSeconds(10), cancellationToken);
                break;
            }
            catch (SocketException e)
            {
                _logger.Error(e, "Socket Exception");
            }

            await Task.Delay(3000).ConfigureAwait(false);
        }

        return socket;
    }

    public IAxisService GetService() => _axisServiceRemotingClient ?? throw new NullReferenceException();

    protected override async ValueTask OnDisposeAsync()
    {
        if (_multiplexer is not null) await _multiplexer.DisposeAsync();
        if (_bridgeConnection is not null) await _bridgeConnection.DisposeAsync();
        if (_batchActionDispatcher is not null) await _batchActionDispatcher.DisposeAsync();
        _cap?.Dispose();
        _socket?.Dispose();
    }
}
