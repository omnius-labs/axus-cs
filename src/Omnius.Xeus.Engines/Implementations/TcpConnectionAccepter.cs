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
using Omnius.Xeus.Engines.Internal;

namespace Omnius.Xeus.Engines
{
    public sealed partial class TcpConnectionAccepter : AsyncDisposableBase, IConnectionAccepter
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly TcpConnectionAccepterOptions _options;

        private TcpListenerManager? _tcpListenerManager;
        private readonly AsyncLock _asyncLock = new();

        public TcpConnectionAccepter(TcpConnectionAccepterOptions options)
        {
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
                    _tcpListenerManager = await TcpListenerManager.CreateAsync(_options.ListenAddresses, _options.UseUpnp, _options.UpnpClientFactory, cancellationToken);
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

                var bridgeConnectionOptions = new BridgeConnectionOptions
                {
                    MaxReceiveByteCount = 1024 * 1024 * 256,
                    BatchActionDispatcher = _options.BatchActionDispatcher,
                    BytesPool = BytesPool.Shared,
                };
                var bridgeConnection = new BridgeConnection(cap, bridgeConnectionOptions);
                return new ConnectionAcceptedResult(bridgeConnection, address);
            }
        }

        public async ValueTask<OmniAddress[]> GetListenEndpointsAsync(CancellationToken cancellationToken = default)
        {
            var results = new List<OmniAddress>();

            var globalIpAddresses = IpAddressHelper.GetMyGlobalIpAddresses();

            foreach (var listenAddress in _options.ListenAddresses ?? Enumerable.Empty<OmniAddress>())
            {
                if (!listenAddress.TryGetTcpEndpoint(out var listenIpAddress, out var port)) continue;

#if DEBUG
                results.Add(OmniAddress.CreateTcpEndpoint(listenIpAddress, port));
#endif

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
            }

            return results.ToArray();
        }
    }
}
