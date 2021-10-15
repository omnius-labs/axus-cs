using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Core.Net.Caps;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Connections.Bridge;
using Omnius.Core.Net.Proxies;
using Omnius.Core.Tasks;

namespace Omnius.Xeus.Service.Engines
{
    public sealed partial class TcpConnectionConnector : AsyncDisposableBase, IConnectionConnector
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IBandwidthLimiter _senderBandwidthLimiter;
        private readonly IBandwidthLimiter _receiverBandwidthLimiter;
        private readonly ISocks5ProxyClientFactory _socks5ProxyClientFactory;
        private readonly IHttpProxyClientFactory _httpProxyClientFactory;
        private readonly IBatchActionDispatcher _batchActionDispatcher;
        private readonly IBytesPool _bytesPool;
        private readonly TcpConnectionConnectorOptions _options;

        private const int MaxReceiveByteCount = 1024 * 1024 * 256;

        public static async ValueTask<TcpConnectionConnector> CreateAsync(IBandwidthLimiter senderBandwidthLimiter, IBandwidthLimiter receiverBandwidthLimiter, ISocks5ProxyClientFactory socks5ProxyClientFactory, IHttpProxyClientFactory httpProxyClientFactory, IBatchActionDispatcher batchActionDispatcher, IBytesPool bytesPool, TcpConnectionConnectorOptions options, CancellationToken cancellationToken = default)
        {
            var tcpConnectionConnector = new TcpConnectionConnector(senderBandwidthLimiter, receiverBandwidthLimiter, socks5ProxyClientFactory, httpProxyClientFactory, batchActionDispatcher, bytesPool, options);
            return tcpConnectionConnector;
        }

        private TcpConnectionConnector(IBandwidthLimiter senderBandwidthLimiter, IBandwidthLimiter receiverBandwidthLimiter, ISocks5ProxyClientFactory socks5ProxyClientFactory, IHttpProxyClientFactory httpProxyClientFactory, IBatchActionDispatcher batchActionDispatcher, IBytesPool bytesPool, TcpConnectionConnectorOptions options)
        {
            _senderBandwidthLimiter = senderBandwidthLimiter;
            _receiverBandwidthLimiter = receiverBandwidthLimiter;
            _socks5ProxyClientFactory = socks5ProxyClientFactory;
            _httpProxyClientFactory = httpProxyClientFactory;
            _batchActionDispatcher = batchActionDispatcher;
            _bytesPool = bytesPool;
            _options = options;
        }

        protected override async ValueTask OnDisposeAsync()
        {
        }

        public async ValueTask<IConnection?> ConnectAsync(OmniAddress address, CancellationToken cancellationToken = default)
        {
            var cap = await this.ConnectCapAsync(address, cancellationToken);
            if (cap == null) return null;

            var bridgeConnectionOptions = new BridgeConnectionOptions(MaxReceiveByteCount);
            var bridgeConnection = new BridgeConnection(cap, _senderBandwidthLimiter, _receiverBandwidthLimiter, _batchActionDispatcher, _bytesPool, bridgeConnectionOptions);
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

                if (_options.Proxy?.Address is not null && _options.Proxy.Address.TryGetTcpEndpoint(out var proxyAddress, out ushort proxyPort, true))
                {
                    if (_socks5ProxyClientFactory is not null && _options.Proxy.Type == TcpProxyType.Socks5Proxy)
                    {
                        var socket = await ConnectSocketAsync(new IPEndPoint(proxyAddress, proxyPort), cancellationToken);
                        if (socket == null) return null;

                        disposableList.Add(socket);

                        var proxy = _socks5ProxyClientFactory.Create(ipAddress.ToString(), port);
                        await proxy.ConnectAsync(socket, cancellationToken);

                        var cap = new SocketCap(socket);
                        disposableList.Add(cap);

                        return cap;
                    }
                    else if (_httpProxyClientFactory is not null && _options.Proxy.Type == TcpProxyType.HttpProxy)
                    {
                        var socket = await ConnectSocketAsync(new IPEndPoint(proxyAddress, proxyPort), cancellationToken);
                        if (socket == null) return null;

                        disposableList.Add(socket);

                        var proxy = _httpProxyClientFactory.Create(ipAddress.ToString(), port);
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
