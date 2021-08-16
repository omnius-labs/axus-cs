using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Core.Net.Caps;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Connections.Bridge;
using Omnius.Core.Net.Connections.Multiplexer;
using Omnius.Core.Net.Connections.Multiplexer.V1;
using Omnius.Core.Net.Connections.Secure;
using Omnius.Core.Net.Connections.Secure.V1;
using Omnius.Core.Net.Proxies;
using Omnius.Core.Net.Upnp;
using Omnius.Core.Tasks;

namespace Omnius.Xeus.Engines
{
    public sealed partial class TcpConnectionConnector : AsyncDisposableBase, IConnectionConnector
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly TcpConnectionConnectorOptions _options;

        public TcpConnectionConnector(TcpConnectionConnectorOptions options)
        {
            _options = options;
        }

        protected override async ValueTask OnDisposeAsync()
        {
        }

        public async ValueTask<IConnection?> ConnectAsync(OmniAddress address, CancellationToken cancellationToken = default)
        {
            var cap = await this.ConnectCapAsync(address, cancellationToken);
            if (cap == null) return null;

            var bridgeConnectionOptions = new BridgeConnectionOptions
            {
                MaxReceiveByteCount = 1024 * 1024 * 256,
                BatchActionDispatcher = _options.BatchActionDispatcher,
                BytesPool = BytesPool.Shared,
            };
            var bridgeConnection = new BridgeConnection(cap, bridgeConnectionOptions);
            return bridgeConnection;
        }

        public async ValueTask<ICap?> ConnectCapAsync(OmniAddress address, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposingRequested();

            if (!address.TryGetTcpEndpoint(out var ipAddress, out ushort port)) return null;

            var disposableList = new List<IDisposable>();

            try
            {
#if !DEBUG
                if (!IsGlobalIpAddress(ipAddress)) return null;
#endif

                if (_options.Proxy?.Address is not null)
                {
                    if (!_options.Proxy.Address.TryGetTcpEndpoint(out var proxyAddress, out ushort proxyPort, true)) return null;

                    if (_options.Socks5ProxyClientFactory is not null && _options.Proxy.Type == TcpProxyType.Socks5Proxy)
                    {
                        var socket = await ConnectSocketAsync(new IPEndPoint(proxyAddress, proxyPort), cancellationToken);
                        if (socket == null) return null;

                        disposableList.Add(socket);

                        var proxy = _options.Socks5ProxyClientFactory.Create(ipAddress.ToString(), port);
                        await proxy.ConnectAsync(socket, cancellationToken);

                        var cap = new SocketCap(socket);
                        disposableList.Add(cap);

                        return cap;
                    }
                    else if (_options.HttpProxyClientFactory is not null && _options.Proxy.Type == TcpProxyType.HttpProxy)
                    {
                        var socket = await ConnectSocketAsync(new IPEndPoint(proxyAddress, proxyPort), cancellationToken);
                        if (socket == null) return null;

                        disposableList.Add(socket);

                        var proxy = _options.HttpProxyClientFactory.Create(ipAddress.ToString(), port);
                        await proxy.ConnectAsync(socket, cancellationToken);

                        var cap = new SocketCap(socket);
                        disposableList.Add(cap);

                        return cap;
                    }
                }
                else
                {
                    var socket = await ConnectSocketAsync(new IPEndPoint(ipAddress, port), cancellationToken);
                    if (socket == null) return null;

                    disposableList.Add(socket);

                    var cap = new SocketCap(socket);
                    disposableList.Add(cap);

                    return cap;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);

                foreach (var item in disposableList)
                {
                    item.Dispose();
                }
            }

            return null;
        }

        private static async ValueTask<Socket?> ConnectSocketAsync(IPEndPoint remoteEndPoint, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);

            Socket? socket = null;

            try
            {
                socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = 1000 * 10,
                    ReceiveTimeout = 1000 * 10,
                };
                await socket.ConnectAsync(remoteEndPoint, TimeSpan.FromSeconds(3), cancellationToken);

                return socket;
            }
            catch (SocketException)
            {
                socket?.Dispose();

                return null;
            }
        }
    }
}
