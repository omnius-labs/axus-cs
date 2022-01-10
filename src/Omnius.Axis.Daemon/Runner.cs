using System.Net.Sockets;
using Omnius.Axis.Remoting;
using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Core.Net.Caps;
using Omnius.Core.Net.Connections.Bridge;
using Omnius.Core.Net.Connections.Multiplexer;
using Omnius.Core.Net.Connections.Multiplexer.V1;
using Omnius.Core.RocketPack.Remoting;
using Omnius.Core.Tasks;

namespace Omnius.Axis.Daemon;

public static partial class Runner
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    public static async ValueTask EventLoopAsync(string databaseDirectoryPath, OmniAddress listenAddress, CancellationToken cancellationToken = default)
    {
        await using var service = await AxisService.CreateAsync(databaseDirectoryPath, cancellationToken);
        using var tcpListenerManager = new TcpListenerManager(listenAddress, cancellationToken);

        var tasks = new List<Task>();

        try
        {
            for (; ; )
            {
                var socket = await tcpListenerManager.AcceptSocketAsync();
                var task = InternalEventLoopAsync(service, socket, cancellationToken);
                tasks.Add(task);
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug("OperationCanceledException", e);
        }
        finally
        {
            await Task.WhenAll(tasks);
        }
    }

    private static async Task InternalEventLoopAsync(AxisService service, Socket socket, CancellationToken cancellationToken = default)
    {
        using var socketCap = new SocketCap(socket);

        var bytesPool = BytesPool.Shared;
        await using var batchActionDispatcher = new BatchActionDispatcher(TimeSpan.FromMilliseconds(10));

        var bridgeConnectionOptions = new BridgeConnectionOptions(int.MaxValue);
        await using var bridgeConnection = new BridgeConnection(socketCap, null, null, batchActionDispatcher, bytesPool, bridgeConnectionOptions);

        var multiplexerOption = new OmniConnectionMultiplexerOptions(OmniConnectionMultiplexerType.Accepted, TimeSpan.FromMinutes(1), 3, 1024 * 1024 * 4, 3);
        await using var multiplexer = OmniConnectionMultiplexer.CreateV1(bridgeConnection, batchActionDispatcher, bytesPool, multiplexerOption);

        await multiplexer.HandshakeAsync(cancellationToken);

        var errorMessageFactory = new DefaultErrorMessageFactory();
        var listenerFactory = new RocketRemotingListenerFactory<DefaultErrorMessage>(multiplexer, errorMessageFactory, bytesPool);

        try
        {
            _logger.Debug("InternalEventLoopAsync: Start");

            var server = new AxisServiceRemoting.Server<DefaultErrorMessage>(service, listenerFactory, bytesPool);

            var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            using var onClose = bridgeConnection.Events.OnClosed.Subscribe(() => linkedCancellationTokenSource.Cancel());

            await server.EventLoopAsync(linkedCancellationTokenSource.Token);
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
        finally
        {
            _logger.Debug("InternalEventLoopAsync: End");
        }
    }
}
