using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Extensions;
using Omnius.Core.Network;
using Omnius.Core.Network.Caps;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Xeus.Engines.Models;

namespace Omnius.Xeus.Engines.Connectors.Internal
{
    internal sealed class InternalTcpConnector : AsyncDisposableBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly TcpConnectingOptions _tcpConnectingOptions;
        private readonly TcpAcceptingOptions _tcpAcceptingOptions;
        private readonly ISocks5ProxyClientFactory _socks5ProxyClientFactory;
        private readonly IHttpProxyClientFactory _httpProxyClientFactory;
        private readonly IUpnpClientFactory _upnpClientFactory;
        private readonly IBytesPool _bytesPool;

        private readonly List<TcpListener> _tcpListeners = new List<TcpListener>();

        private readonly Random _random = new Random();

        public sealed class InternalTcpConnectorFactory
        {
            public async ValueTask<InternalTcpConnector> CreateAsync(TcpConnectingOptions tcpConnectOptions, TcpAcceptingOptions tcpAcceptOptions, ISocks5ProxyClientFactory socks5ProxyClientFactory, IHttpProxyClientFactory httpProxyClientFactory, IUpnpClientFactory upnpClientFactory, IBytesPool bytesPool)
            {
                var result = new InternalTcpConnector(tcpConnectOptions, tcpAcceptOptions, socks5ProxyClientFactory, httpProxyClientFactory, upnpClientFactory, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static InternalTcpConnectorFactory Factory { get; } = new InternalTcpConnectorFactory();

        internal InternalTcpConnector(TcpConnectingOptions tcpConnectOptions, TcpAcceptingOptions tcpAcceptOptions, ISocks5ProxyClientFactory socks5ProxyClientFactory, IHttpProxyClientFactory httpProxyClientFactory, IUpnpClientFactory upnpClientFactory, IBytesPool bytesPool)
        {
            _tcpConnectingOptions = tcpConnectOptions;
            _tcpAcceptingOptions = tcpAcceptOptions;
            _socks5ProxyClientFactory = socks5ProxyClientFactory;
            _httpProxyClientFactory = httpProxyClientFactory;
            _upnpClientFactory = upnpClientFactory;

            _bytesPool = bytesPool;
        }

        internal async ValueTask InitAsync()
        {
            await this.StartTcpListen();
        }

        protected override async ValueTask OnDisposeAsync()
        {
            await this.StopTcpListen();
        }

        private async ValueTask StartTcpListen()
        {
            var listenAddressSet = new HashSet<OmniAddress>(_tcpAcceptingOptions.ListenAddresses.ToArray());
            var useUpnp = _tcpAcceptingOptions.UseUpnp;

            IUpnpClient? upnpClient = null;

            try
            {
                // TcpListenerの追加処理
                foreach (var listenAddress in listenAddressSet)
                {
                    if (!listenAddress.TryParseTcpEndpoint(out var ipAddress, out ushort port, false)) continue;

                    var tcpListener = new TcpListener(ipAddress, port);
                    tcpListener.Start(3);

                    _tcpListeners.Add(tcpListener);

                    if (useUpnp)
                    {
                        // "0.0.0.0"以外はUPnPでのポート開放対象外
                        if (ipAddress == IPAddress.Any)
                        {
                            if (upnpClient == null)
                            {
                                upnpClient = _upnpClientFactory.Create();
                                await upnpClient.ConnectAsync();
                            }

                            await upnpClient.OpenPortAsync(UpnpProtocolType.Tcp, port, port, "Xeus");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);

                throw;
            }
            finally
            {
                if (upnpClient != null)
                {
                    upnpClient.Dispose();
                }
            }
        }

        private async ValueTask StopTcpListen()
        {
            var useUpnp = _tcpAcceptingOptions.UseUpnp;

            IUpnpClient? upnpClient = null;

            try
            {
                foreach (var tcpListener in _tcpListeners)
                {
                    var ipEndpoint = (IPEndPoint)tcpListener.LocalEndpoint;

                    tcpListener.Stop();

                    if (useUpnp)
                    {
                        // "0.0.0.0"以外はUPnPでのポート開放対象外
                        if (ipEndpoint.Address == IPAddress.Any)
                        {
                            if (upnpClient == null)
                            {
                                upnpClient = _upnpClientFactory.Create();
                                await upnpClient.ConnectAsync();
                            }

                            await upnpClient.ClosePortAsync(UpnpProtocolType.Tcp, ipEndpoint.Port);
                        }
                    }
                }

                _tcpListeners.Clear();
            }
            catch (Exception e)
            {
                _logger.Error(e);

                throw;
            }
            finally
            {
                if (upnpClient != null)
                {
                    upnpClient.Dispose();
                }
            }
        }

        private static readonly ReadOnlyMemory<byte> _ipAddress_10_0_0_0 = IPAddress.Parse("10.0.0.0").GetAddressBytes();
        private static readonly ReadOnlyMemory<byte> _ipAddress_10_255_255_255 = IPAddress.Parse("10.255.255.255").GetAddressBytes();
        private static readonly ReadOnlyMemory<byte> _ipAddress_172_16_0_0 = IPAddress.Parse("172.16.0.0").GetAddressBytes();
        private static readonly ReadOnlyMemory<byte> _ipAddress_172_31_255_255 = IPAddress.Parse("172.31.255.255").GetAddressBytes();
        private static readonly ReadOnlyMemory<byte> _ipAddress_127_0_0_0 = IPAddress.Parse("127.0.0.0").GetAddressBytes();
        private static readonly ReadOnlyMemory<byte> _ipAddress_127_255_255_255 = IPAddress.Parse("127.255.255.255").GetAddressBytes();
        private static readonly ReadOnlyMemory<byte> _ipAddress_192_168_0_0 = IPAddress.Parse("192.168.0.0").GetAddressBytes();
        private static readonly ReadOnlyMemory<byte> _ipAddress_192_168_255_255 = IPAddress.Parse("192.168.255.255").GetAddressBytes();
        private static readonly ReadOnlyMemory<byte> _ipAddress_169_254_0_0 = IPAddress.Parse("169.254.0.0").GetAddressBytes();
        private static readonly ReadOnlyMemory<byte> _ipAddress_169_254_255_255 = IPAddress.Parse("169.254.255.255").GetAddressBytes();

        private static bool IsGlobalIpAddress(IPAddress ipAddress)
        {
            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                if (ipAddress == IPAddress.Any || ipAddress == IPAddress.Loopback || ipAddress == IPAddress.Broadcast) return false;

                var bytes = ipAddress.GetAddressBytes();

                // Loopback Address
                if (BytesOperations.Compare(bytes, _ipAddress_127_0_0_0.Span) >= 0
                    && BytesOperations.Compare(bytes, _ipAddress_127_255_255_255.Span) <= 0)
                {
                    return false;
                }

                // Class A
                if (BytesOperations.Compare(bytes, _ipAddress_10_0_0_0.Span) >= 0
                    && BytesOperations.Compare(bytes, _ipAddress_10_255_255_255.Span) <= 0)
                {
                    return false;
                }

                // Class B
                if (BytesOperations.Compare(bytes, _ipAddress_172_16_0_0.Span) >= 0
                    && BytesOperations.Compare(bytes, _ipAddress_172_31_255_255.Span) <= 0)
                {
                    return false;
                }

                // Class C
                if (BytesOperations.Compare(bytes, _ipAddress_192_168_0_0.Span) >= 0
                    && BytesOperations.Compare(bytes, _ipAddress_192_168_255_255.Span) <= 0)
                {
                    return false;
                }

                // Link Local Address
                if (BytesOperations.Compare(bytes, _ipAddress_169_254_0_0.Span) >= 0
                    && BytesOperations.Compare(bytes, _ipAddress_169_254_255_255.Span) <= 0)
                {
                    return false;
                }
            }

            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (ipAddress == IPAddress.IPv6Any || ipAddress == IPAddress.IPv6Loopback || ipAddress == IPAddress.IPv6None
                    || ipAddress.IsIPv4MappedToIPv6 || ipAddress.IsIPv6LinkLocal || ipAddress.IsIPv6Multicast || ipAddress.IsIPv6SiteLocal || ipAddress.IsIPv6Teredo)
                {
                    return false;
                }
            }

            return true;
        }

        private static IEnumerable<IPAddress> GetMyGlobalIpAddresses()
        {
            var list = new HashSet<IPAddress>();

            try
            {
                list.UnionWith(Dns.GetHostAddresses(Dns.GetHostName()).Where(n => IsGlobalIpAddress(n)));
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return list;
        }

        private static async ValueTask<Socket?> ConnectAsync(IPEndPoint remoteEndPoint, CancellationToken cancellationToken = default)
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

        public async ValueTask<ICap?> ConnectAsync(OmniAddress address, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposingRequested();

            var config = _tcpConnectingOptions;
            if (config == null || !config.Enabled) return null;

            if (!address.TryParseTcpEndpoint(out var ipAddress, out ushort port)) return null;

            var disposableList = new List<IDisposable>();

            try
            {
#if !DEBUG
                if (!IsGlobalIpAddress(ipAddress)) return null;
#endif

                if (config.ProxyOptions?.Address is not null)
                {
                    if (!config.ProxyOptions.Address.TryParseTcpEndpoint(out var proxyAddress, out ushort proxyPort, true)) return null;

                    if (config.ProxyOptions.Type == TcpProxyType.Socks5Proxy)
                    {
                        var socket = await ConnectAsync(new IPEndPoint(proxyAddress, proxyPort));
                        if (socket == null) return null;

                        disposableList.Add(socket);

                        var proxy = _socks5ProxyClientFactory.Create(ipAddress.ToString(), port);
                        await proxy.ConnectAsync(socket, cancellationToken);

                        var cap = new SocketCap(socket);
                        disposableList.Add(cap);

                        return cap;
                    }
                    else if (config.ProxyOptions.Type == TcpProxyType.HttpProxy)
                    {
                        var socket = await ConnectAsync(new IPEndPoint(proxyAddress, proxyPort));
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
                    var socket = await ConnectAsync(new IPEndPoint(ipAddress, port));
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

        public async ValueTask<(ICap?, OmniAddress?)> AcceptAsync(CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposingRequested();

            var garbages = new List<IDisposable>();

            try
            {
                var config = _tcpAcceptingOptions;
                if (config == null || !config.Enabled) return default;

                var tcpListeners = _tcpListeners.ToArray();
                _random.Shuffle(tcpListeners);

                foreach (var tcpListener in tcpListeners)
                {
                    var socket = await tcpListener.AcceptSocketAsync();
                    if (socket is null || socket.RemoteEndPoint is null) continue;

                    garbages.Add(socket);

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

                    return (new SocketCap(socket), address);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);

                foreach (var item in garbages)
                {
                    item.Dispose();
                }
            }

            return default;
        }

        public async ValueTask<OmniAddress[]> GetListenEndpointsAsync(CancellationToken cancellationToken = default)
        {
            var config = _tcpAcceptingOptions;
            if (config == null || !config.Enabled) return Array.Empty<OmniAddress>();

            var results = new List<OmniAddress>();

            var globalIpAddresses = GetMyGlobalIpAddresses();

            foreach (var listenAddress in config.ListenAddresses)
            {
                if (!listenAddress.TryParseTcpEndpoint(out var listenIpAddress, out var port)) continue;

                if (listenIpAddress.AddressFamily == AddressFamily.InterNetwork && listenIpAddress == IPAddress.Any)
                {
                    foreach (var globalIpAddress in globalIpAddresses.Select(n => n.AddressFamily == AddressFamily.InterNetwork))
                    {
                        results.Add(OmniAddress.CreateTcpEndpoint(listenIpAddress, port));
                    }
                }
                else if (listenIpAddress.AddressFamily == AddressFamily.InterNetworkV6 && listenIpAddress == IPAddress.IPv6Any)
                {
                    foreach (var globalIpAddress in globalIpAddresses.Select(n => n.AddressFamily == AddressFamily.InterNetworkV6))
                    {
                        results.Add(OmniAddress.CreateTcpEndpoint(listenIpAddress, port));
                    }
                }
            }

            return results.ToArray();
        }
    }
}
