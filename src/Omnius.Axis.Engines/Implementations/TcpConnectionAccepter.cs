using System.Net;
using System.Net.Sockets;
using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Core.Net.Caps;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Connections.Bridge;
using Omnius.Core.Net.Upnp;
using Omnius.Core.Tasks;

namespace Omnius.Axis.Engines;

public sealed partial class TcpConnectionAccepter : AsyncDisposableBase, IConnectionAccepter
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IBandwidthLimiter _senderBandwidthLimiter;
    private readonly IBandwidthLimiter _receiverBandwidthLimiter;
    private readonly IUpnpClientFactory _upnpClientFactory;
    private readonly IBatchActionDispatcher _batchActionDispatcher;
    private readonly IBytesPool _bytesPool;
    private readonly TcpConnectionAccepterOptions _options;

    private TcpListenerManager? _tcpListenerManager;
    private readonly AsyncLock _asyncLock = new();

    private const int MaxReceiveByteCount = 1024 * 1024 * 256;

    public static async ValueTask<TcpConnectionAccepter> CreateAsync(IBandwidthLimiter senderBandwidthLimiter, IBandwidthLimiter receiverBandwidthLimiter, IUpnpClientFactory upnpClientFactory, IBatchActionDispatcher batchActionDispatcher, IBytesPool bytesPool, TcpConnectionAccepterOptions options, CancellationToken cancellationToken = default)
    {
        var tcpConnectionAccepter = new TcpConnectionAccepter(senderBandwidthLimiter, receiverBandwidthLimiter, upnpClientFactory, batchActionDispatcher, bytesPool, options);
        return tcpConnectionAccepter;
    }

    private TcpConnectionAccepter(IBandwidthLimiter senderBandwidthLimiter, IBandwidthLimiter receiverBandwidthLimiter, IUpnpClientFactory upnpClientFactory, IBatchActionDispatcher batchActionDispatcher, IBytesPool bytesPool, TcpConnectionAccepterOptions options)
    {
        _senderBandwidthLimiter = senderBandwidthLimiter;
        _receiverBandwidthLimiter = receiverBandwidthLimiter;
        _upnpClientFactory = upnpClientFactory;
        _batchActionDispatcher = batchActionDispatcher;
        _bytesPool = bytesPool;
        _options = options;
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        _tcpListenerManager = await TcpListenerManager.CreateAsync(_options.ListenAddress, _options.UseUpnp, _upnpClientFactory, cancellationToken);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        if (_tcpListenerManager is not null) await _tcpListenerManager.DisposeAsync();
    }

    public async ValueTask<ConnectionAcceptedResult?> AcceptAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var (cap, address) = await this.AcceptCapAsync(cancellationToken);
            if (cap is null || address is null) return null;

            var bridgeConnectionOptions = new BridgeConnectionOptions(MaxReceiveByteCount);
            var bridgeConnection = new BridgeConnection(cap, _senderBandwidthLimiter, _receiverBandwidthLimiter, _batchActionDispatcher, _bytesPool, bridgeConnectionOptions);
            return new ConnectionAcceptedResult(bridgeConnection, address);
        }
    }

    private async ValueTask<(ICap?, OmniAddress?)> AcceptCapAsync(CancellationToken cancellationToken = default)
    {
        var disposableList = new List<IDisposable>();

        try
        {
            var socket = await _tcpListenerManager!.AcceptAsync(cancellationToken);
            if (socket is null || socket.RemoteEndPoint is null) return (null, null);

            var endpoint = (IPEndPoint)socket.RemoteEndPoint;

            OmniAddress address;

            if (endpoint.AddressFamily == AddressFamily.InterNetwork)
            {
                address = OmniAddress.CreateTcpEndpoint(endpoint.Address, (ushort)endpoint.Port);
            }
            else if (endpoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                address = OmniAddress.CreateTcpEndpoint(endpoint.Address, (ushort)endpoint.Port);
            }
            else
            {
                throw new NotSupportedException();
            }

            var cap = new SocketCap(socket);

            return (cap, address);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");

            foreach (var item in disposableList)
            {
                item.Dispose();
            }
        }

        return (null, null);
    }

    public async ValueTask<OmniAddress[]> GetListenEndpointsAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.ListenAddress.TryParseTcpEndpoint(out var listenIpAddress, out var port))
        {
            return Array.Empty<OmniAddress>();
        }

        var results = new List<OmniAddress>();

#if DEBUG
        results.Add(OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, port));
#endif

        if (_tcpListenerManager is null)
        {
            return results.ToArray();
        }

        var globalIpAddresses = await _tcpListenerManager.GetMyGlobalIpAddressesAsync(cancellationToken);

        if (listenIpAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            foreach (var globalIpAddress in globalIpAddresses.Where(n => n.AddressFamily == AddressFamily.InterNetwork))
            {
                results.Add(OmniAddress.CreateTcpEndpoint(globalIpAddress, port));
            }
        }
        else if (listenIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            foreach (var globalIpAddress in globalIpAddresses.Where(n => n.AddressFamily == AddressFamily.InterNetworkV6))
            {
                results.Add(OmniAddress.CreateTcpEndpoint(globalIpAddress, port));
            }
        }

        return results.ToArray();
    }
}
