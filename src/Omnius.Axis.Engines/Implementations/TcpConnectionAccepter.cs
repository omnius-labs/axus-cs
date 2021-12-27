using System.Net;
using System.Net.Sockets;
using LazyCache;
using Microsoft.Extensions.Caching.Memory;
using Omnius.Axis.Engines.Internal;
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

    private readonly CachingService _cache;
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

        _cache = new CachingService();
    }

    protected override async ValueTask OnDisposeAsync()
    {
        if (_tcpListenerManager is not null) await _tcpListenerManager.DisposeAsync();
    }

    public async ValueTask<ConnectionAcceptedResult?> AcceptAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            if (_tcpListenerManager is null)
            {
                _tcpListenerManager = await TcpListenerManager.CreateAsync(_options.ListenAddress, _options.UseUpnp, _upnpClientFactory, cancellationToken);
            }

            var socket = await _tcpListenerManager.AcceptAsync(cancellationToken);
            if (socket is null || socket.RemoteEndPoint is null) return null;

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

            var bridgeConnectionOptions = new BridgeConnectionOptions(MaxReceiveByteCount);
            var bridgeConnection = new BridgeConnection(cap, _senderBandwidthLimiter, _receiverBandwidthLimiter, _batchActionDispatcher, _bytesPool, bridgeConnectionOptions);
            return new ConnectionAcceptedResult(bridgeConnection, address);
        }
    }

    public async ValueTask<OmniAddress[]> GetListenEndpointsAsync(CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions() { SlidingExpiration = TimeSpan.FromMinutes(30) };
        return await _cache.GetOrAddAsync("GetListenEndpointsAsync", async (_) => await this.InternalGetListenEndpointsAsync(cancellationToken), options);
    }

    private async ValueTask<OmniAddress[]> InternalGetListenEndpointsAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.ListenAddress.TryGetTcpEndpoint(out var listenIpAddress, out var port)) Array.Empty<OmniAddress>();

        var results = new List<OmniAddress>();
        results.Add(OmniAddress.CreateTcpEndpoint(listenIpAddress, port));

        var globalIpAddresses = await this.GetMyGlobalIpAddressesAsync(cancellationToken);

        if (listenIpAddress.AddressFamily == AddressFamily.InterNetwork && listenIpAddress == IPAddress.Any)
        {
            foreach (var globalIpAddress in globalIpAddresses.Where(n => n.AddressFamily == AddressFamily.InterNetwork))
            {
                results.Add(OmniAddress.CreateTcpEndpoint(globalIpAddress, port));
            }
        }
        else if (listenIpAddress.AddressFamily == AddressFamily.InterNetworkV6 && listenIpAddress == IPAddress.IPv6Any)
        {
            foreach (var globalIpAddress in globalIpAddresses.Where(n => n.AddressFamily == AddressFamily.InterNetworkV6))
            {
                results.Add(OmniAddress.CreateTcpEndpoint(globalIpAddress, port));
            }
        }

        return results.ToArray();
    }

    public async ValueTask<IEnumerable<IPAddress>> GetMyGlobalIpAddressesAsync(CancellationToken cancellationToken = default)
    {
        var list = new HashSet<IPAddress>();

        try
        {
            if (_options.UseUpnp)
            {
                using var upnpClient = _upnpClientFactory.Create();
                await upnpClient.ConnectAsync(cancellationToken);
                var externalIp = IPAddress.Parse(await upnpClient.GetExternalIpAddressAsync(cancellationToken));

                if (IpAddressHelper.IsGlobalIpAddress(externalIp))
                {
                    list.Add(externalIp);
                }
            }

            foreach (var ipAddress in Dns.GetHostAddresses(Dns.GetHostName()))
            {
#if !DEBUG
                if (!Internal.IpAddressHelper.IsGlobalIpAddress(ipAddress)) continue;
#endif

                list.Add(ipAddress);
            }
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }

        return list;
    }
}
