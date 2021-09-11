using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Core.Net.Caps;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Connections.Bridge;
using Omnius.Core.Net.Upnp;
using Omnius.Core.Tasks;
using Omnius.Xeus.Service.Engines.Internal;

namespace Omnius.Xeus.Service.Engines
{
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

        private const int MaxReceiveByteCount = 1024 * 1024 * 8;

        public TcpConnectionAccepter(IBandwidthLimiter senderBandwidthLimiter, IBandwidthLimiter receiverBandwidthLimiter, IUpnpClientFactory upnpClientFactory, IBatchActionDispatcher batchActionDispatcher, IBytesPool bytesPool, TcpConnectionAccepterOptions options)
        {
            _senderBandwidthLimiter = senderBandwidthLimiter;
            _receiverBandwidthLimiter = receiverBandwidthLimiter;
            _upnpClientFactory = upnpClientFactory;
            _batchActionDispatcher = batchActionDispatcher;
            _bytesPool = bytesPool;
            _options = options;
        }

        protected override async ValueTask OnDisposeAsync()
        {
            using (await _asyncLock.LockAsync())
            {
                if (_tcpListenerManager is not null) await _tcpListenerManager.DisposeAsync();
            }
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
            if (!_options.ListenAddress.TryGetTcpEndpoint(out var listenIpAddress, out var port)) Array.Empty<OmniAddress>();

            var results = new List<OmniAddress>();

#if DEBUG
            results.Add(OmniAddress.CreateTcpEndpoint(listenIpAddress, port));
#endif

            var globalIpAddresses = IpAddressHelper.GetMyGlobalIpAddresses();

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
    }
}
