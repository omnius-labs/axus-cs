using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Extensions;
using Omnius.Core.Network;
using Omnius.Core.Network.Caps;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;

namespace Omnius.Xeus.Service.Drivers.Internal
{
    internal sealed class TcpConnector : AsyncDisposableBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly TcpConnectOptions _tcpConnectOptions;
        private readonly TcpAcceptOptions _tcpAcceptOptions;
        private readonly IBytesPool _bytesPool;

        private readonly List<TcpListener> _tcpListeners = new List<TcpListener>();

        private readonly Random _random = new Random();

        private readonly AsyncLock _asyncLock = new AsyncLock();

        public sealed class TcpConnectorFactory
        {
            public async ValueTask<TcpConnector> CreateAsync(TcpConnectOptions tcpConnectOptions, TcpAcceptOptions tcpAcceptOptions, IBytesPool bytesPool)
            {
                var result = new TcpConnector(tcpConnectOptions, tcpAcceptOptions, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static TcpConnectorFactory Factory { get; } = new TcpConnectorFactory();

        internal TcpConnector(TcpConnectOptions tcpConnectOptions, TcpAcceptOptions tcpAcceptOptions, IBytesPool bytesPool)
        {
            _tcpConnectOptions = tcpConnectOptions;
            _tcpAcceptOptions = tcpAcceptOptions;
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
            using (await _asyncLock.LockAsync())
            {
                var listenAddressSet = new HashSet<OmniAddress>(_tcpAcceptOptions.ListenAddresses.ToArray());
                var useUpnp = _tcpAcceptOptions.UseUpnp;

                UpnpClient? upnpClient = null;

                try
                {
                    // TcpListenerの追加処理
                    foreach (var listenAddress in listenAddressSet)
                    {
                        if (!TryGetEndpoint(listenAddress, out var ipAddress, out ushort port, false))
                        {
                            continue;
                        }

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
                                    upnpClient = new UpnpClient();
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

                    throw e;
                }
                finally
                {
                    if (upnpClient != null)
                    {
                        upnpClient.Dispose();
                    }
                }
            }
        }

        private async ValueTask StopTcpListen()
        {
            using (await _asyncLock.LockAsync())
            {
                var useUpnp = _tcpAcceptOptions.UseUpnp;

                UpnpClient? upnpClient = null;

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
                                    upnpClient = new UpnpClient();
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

                    throw e;
                }
                finally
                {
                    if (upnpClient != null)
                    {
                        upnpClient.Dispose();
                    }
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
                if (ipAddress == IPAddress.Any || ipAddress == IPAddress.Loopback || ipAddress == IPAddress.Broadcast)
                {
                    return false;
                }

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

        private static bool TryGetEndpoint(OmniAddress rootAddress, [NotNullWhen(true)] out IPAddress? ipAddress, out ushort port, bool nameResolving = false)
        {
            ipAddress = IPAddress.None;
            port = 0;

            var rootFunction = rootAddress.Parse();

            if (rootFunction == null)
            {
                return false;
            }

            if (rootFunction.Name == "tcp")
            {
                if (!(rootFunction.Arguments.Count == 2
                    && rootFunction.Arguments[0] is OmniAddress.FunctionElement hostFunction
                    && rootFunction.Arguments[1] is OmniAddress.ConstantElement portConstant))
                {
                    return false;
                }

                if (hostFunction.Name == "ip4")
                {
                    if (!(hostFunction.Arguments.Count == 1
                        && hostFunction.Arguments[0] is OmniAddress.ConstantElement ipAddressConstant))
                    {
                        return false;
                    }

                    if (!IPAddress.TryParse(ipAddressConstant.Text, out var temp)
                        || temp.AddressFamily != AddressFamily.InterNetwork)
                    {
                        return false;
                    }

                    ipAddress = temp;
                }
                else if (hostFunction.Name == "ip6")
                {
                    if (!(hostFunction.Arguments.Count == 1
                        && hostFunction.Arguments[0] is OmniAddress.ConstantElement ipAddressConstant))
                    {
                        return false;
                    }

                    if (!IPAddress.TryParse(ipAddressConstant.Text, out var temp)
                        || temp.AddressFamily != AddressFamily.InterNetworkV6)
                    {
                        return false;
                    }

                    ipAddress = temp;
                }
                else if (nameResolving && hostFunction.Name == "dns")
                {
                    if (!(hostFunction.Arguments.Count == 1
                        && hostFunction.Arguments[0] is OmniAddress.ConstantElement hostnameConstant))
                    {
                        return false;
                    }

                    try
                    {
                        var hostEntry = Dns.GetHostEntry(hostnameConstant.Text);

                        if (hostEntry.AddressList.Length == 0)
                        {
                            return false;
                        }

                        ipAddress = hostEntry.AddressList[0];
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e);
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                if (!ushort.TryParse(portConstant.Text, out port))
                {
                    return false;
                }
            }

            return true;
        }

        private static async ValueTask<Socket?> ConnectAsync(IPEndPoint remoteEndPoint)
        {
            return await Task.Run(() =>
            {
                Socket? socket = null;

                try
                {
                    socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                    {
                        SendTimeout = 1000 * 10,
                        ReceiveTimeout = 1000 * 10
                    };
                    socket.Connect(remoteEndPoint);

                    return socket;
                }
                catch (SocketException)
                {
                    if (socket != null)
                    {
                        socket.Dispose();
                    }

                    return null;
                }
            });
        }

        public async ValueTask<ICap?> ConnectAsync(OmniAddress address, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                this.ThrowIfDisposingRequested();

                var config = _tcpConnectOptions;
                if (config == null || !config.Enabled)
                {
                    return null;
                }

                if (!TryGetEndpoint(address, out var ipAddress, out ushort port))
                {
                    return null;
                }

                var disposableList = new List<IDisposable>();

                try
                {
#if !DEBUG
                if (!IsGlobalIpAddress(ipAddress)) return null;
#endif

                    if (config.ProxyOptions != null)
                    {
                        if (!TryGetEndpoint(config.ProxyOptions.Address, out var proxyAddress, out ushort proxyPort, true))
                        {
                            return null;
                        }

                        if (config.ProxyOptions.Type == TcpProxyType.Socks5Proxy)
                        {
                            var socket = await ConnectAsync(new IPEndPoint(proxyAddress, proxyPort));
                            if (socket == null)
                            {
                                return null;
                            }

                            disposableList.Add(socket);

                            var proxy = new Socks5ProxyClient(ipAddress.ToString(), port);
                            await proxy.ConnectAsync(socket, cancellationToken);

                            var cap = new SocketCap(socket);
                            disposableList.Add(cap);

                            return cap;
                        }
                        else if (config.ProxyOptions.Type == TcpProxyType.HttpProxy)
                        {
                            var socket = await ConnectAsync(new IPEndPoint(proxyAddress, proxyPort));
                            if (socket == null)
                            {
                                return null;
                            }

                            disposableList.Add(socket);

                            var proxy = new HttpProxyClient(ipAddress.ToString(), port);
                            await proxy.ConnectAsync(socket, cancellationToken);

                            var cap = new SocketCap(socket);
                            disposableList.Add(cap);

                            return cap;
                        }
                    }
                    else
                    {
                        var socket = await ConnectAsync(new IPEndPoint(ipAddress, port));
                        if (socket == null)
                        {
                            return null;
                        }

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
        }

        public async ValueTask<(ICap?, OmniAddress?)> AcceptAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                this.ThrowIfDisposingRequested();

                var garbages = new List<IDisposable>();

                try
                {
                    var config = _tcpAcceptOptions;
                    if (config == null || !config.Enabled)
                    {
                        return default;
                    }

                    var tcpListeners = _tcpListeners.ToArray();
                    _random.Shuffle(tcpListeners);

                    foreach (var tcpListener in tcpListeners)
                    {
                        var socket = await tcpListener.AcceptSocketAsync();
                        garbages.Add(socket);

                        var endpoint = (IPEndPoint)socket.RemoteEndPoint;

                        OmniAddress address;

                        if (endpoint.AddressFamily == AddressFamily.InterNetwork)
                        {
                            address = new OmniAddress($"tcp(ip4({endpoint.Address},{endpoint.Port}))");
                        }
                        else if (endpoint.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            address = new OmniAddress($"tcp(ip6({endpoint.Address},{endpoint.Port}))");
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
        }

        public async ValueTask<OmniAddress[]> GetListenEndpointsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                var results = new List<OmniAddress>();

                var globalIpAddresses = GetMyGlobalIpAddresses();

                foreach (var listenAddress in _tcpAcceptOptions.ListenAddresses)
                {
                    if (!TryGetEndpoint(listenAddress, out var listenIpAddress, out var port))
                    {
                        continue;
                    }

                    if (listenIpAddress.AddressFamily == AddressFamily.InterNetwork && listenIpAddress == IPAddress.Any)
                    {
                        foreach (var globalIpAddress in globalIpAddresses.Select(n => n.AddressFamily == AddressFamily.InterNetwork))
                        {
                            results.Add(new OmniAddress($"tcp(ip4({listenAddress}),{port})"));
                        }
                    }
                    else if (listenIpAddress.AddressFamily == AddressFamily.InterNetworkV6 && listenIpAddress == IPAddress.IPv6Any)
                    {
                        foreach (var globalIpAddress in globalIpAddresses.Select(n => n.AddressFamily == AddressFamily.InterNetworkV6))
                        {
                            results.Add(new OmniAddress($"tcp(ip6({listenAddress}),{port})"));
                        }
                    }
                }

                return results.ToArray();
            }
        }
    }
}
