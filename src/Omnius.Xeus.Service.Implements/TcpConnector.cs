using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Configuration;
using Omnius.Core.Extensions;
using Omnius.Core.Net.Upnp;
using Omnius.Core.Network;
using Omnius.Core.Network.Caps;
using Omnius.Core.Network.Proxies;
using Omnius.Xeus.Service;
using Omnius.Xeus.Service.Internal;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public sealed class TcpConnector : AsyncDisposableBase, ITcpConnector
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IBufferPool<byte> _bufferPool;

        private readonly TcpConnectOptions _tcpConnectOptions;
        private readonly TcpAcceptOptions _tcpAcceptOptions;

        private readonly List<TcpListener> _tcpListeners = new List<TcpListener>();

        private readonly Random _random = new Random();

        private readonly AsyncLock _asyncLock = new AsyncLock();

        internal TcpConnector(TcpConnectorOptions tcpConnectorOptions, IBufferPool<byte> bufferPool)
        {
            _tcpConnectOptions = tcpConnectorOptions.TcpConnectOptions;
            _tcpAcceptOptions = tcpConnectorOptions.TcpAcceptOptions;
            _bufferPool = bufferPool;
        }

        public static async ValueTask<ITcpConnector> Create(TcpConnectorOptions tcpConnectorOptions, IBufferPool<byte> bufferPool)
        {
            var result = new TcpConnector(tcpConnectorOptions, bufferPool);
            await result.InitAsync();

            return result;
        }

        public async ValueTask InitAsync()
        {
            await this.StartTcpListen();
        }

        protected override async ValueTask OnDisposeAsync()
        {
            await this.StopTcpListen();
        }

        private static bool IsGlobalIpAddress(IPAddress ipAddress)
        {
            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                if (ipAddress == IPAddress.Any || ipAddress == IPAddress.Loopback || ipAddress == IPAddress.Broadcast)
                {
                    return false;
                }
                if (BytesOperations.Compare(ipAddress.GetAddressBytes(), IPAddress.Parse("10.0.0.0").GetAddressBytes()) >= 0
                    && BytesOperations.Compare(ipAddress.GetAddressBytes(), IPAddress.Parse("10.255.255.255").GetAddressBytes()) <= 0)
                {
                    return false;
                }
                if (BytesOperations.Compare(ipAddress.GetAddressBytes(), IPAddress.Parse("172.16.0.0").GetAddressBytes()) >= 0
                    && BytesOperations.Compare(ipAddress.GetAddressBytes(), IPAddress.Parse("172.31.255.255").GetAddressBytes()) <= 0)
                {
                    return false;
                }
                if (BytesOperations.Compare(ipAddress.GetAddressBytes(), IPAddress.Parse("127.0.0.0").GetAddressBytes()) >= 0
                    && BytesOperations.Compare(ipAddress.GetAddressBytes(), IPAddress.Parse("127.255.255.255").GetAddressBytes()) <= 0)
                {
                    return false;
                }
                if (BytesOperations.Compare(ipAddress.GetAddressBytes(), IPAddress.Parse("192.168.0.0").GetAddressBytes()) >= 0
                    && BytesOperations.Compare(ipAddress.GetAddressBytes(), IPAddress.Parse("192.168.255.255").GetAddressBytes()) <= 0)
                {
                    return false;
                }
                if (BytesOperations.Compare(ipAddress.GetAddressBytes(), IPAddress.Parse("169.254.0.0").GetAddressBytes()) >= 0
                    && BytesOperations.Compare(ipAddress.GetAddressBytes(), IPAddress.Parse("169.254.255.255").GetAddressBytes()) <= 0)
                {
                    return false;
                }
            }
            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (ipAddress == IPAddress.IPv6Any || ipAddress == IPAddress.IPv6Loopback || ipAddress == IPAddress.IPv6None)
                {
                    return false;
                }
                if (ipAddress.ToString().StartsWith("fe80:"))
                {
                    return false;
                }
                if (ipAddress.ToString().StartsWith("2001:"))
                {
                    return false;
                }
                if (ipAddress.ToString().StartsWith("2002:"))
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

        public async ValueTask<ConnectorResult> ConnectAsync(OmniAddress address, CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                if (this.IsDisposed)
                {
                    return new ConnectorResult(ConnectorResultType.Failed);
                }

                var config = _tcpConnectOptions;
                if (config == null || !config.Enabled)
                {
                    return new ConnectorResult(ConnectorResultType.Failed);
                }

                if (!TryGetEndpoint(address, out var ipAddress, out ushort port))
                {
                    return new ConnectorResult(ConnectorResultType.Failed);
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
                            return new ConnectorResult(ConnectorResultType.Failed);
                        }

                        if (config.ProxyOptions.Type == TcpProxyType.Socks5Proxy)
                        {
                            var socket = await ConnectAsync(new IPEndPoint(proxyAddress, proxyPort));
                            if (socket == null)
                            {
                                return new ConnectorResult(ConnectorResultType.Failed);
                            }

                            disposableList.Add(socket);

                            var proxy = new Socks5ProxyClient(ipAddress.ToString(), port);
                            await proxy.ConnectAsync(socket, cancellationToken);

                            var cap = new SocketCap(socket, false);
                            disposableList.Add(cap);

                            return new ConnectorResult(ConnectorResultType.Succeeded, cap, address);
                        }
                        else if (config.ProxyOptions.Type == TcpProxyType.HttpProxy)
                        {
                            var socket = await ConnectAsync(new IPEndPoint(proxyAddress, proxyPort));
                            if (socket == null)
                            {
                                return new ConnectorResult(ConnectorResultType.Failed);
                            }

                            disposableList.Add(socket);

                            var proxy = new HttpProxyClient(ipAddress.ToString(), port);
                            await proxy.ConnectAsync(socket, cancellationToken);

                            var cap = new SocketCap(socket, false);
                            disposableList.Add(cap);

                            return new ConnectorResult(ConnectorResultType.Succeeded, cap, address);
                        }
                    }
                    else
                    {
                        var socket = await ConnectAsync(new IPEndPoint(ipAddress, port));
                        if (socket == null)
                        {
                            return new ConnectorResult(ConnectorResultType.Failed);
                        }

                        disposableList.Add(socket);

                        var cap = new SocketCap(socket, false);
                        disposableList.Add(cap);

                        return new ConnectorResult(ConnectorResultType.Succeeded, cap, address);
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

                return new ConnectorResult(ConnectorResultType.Failed);
            }
        }

        public async ValueTask<ConnectorResult> AcceptAsync(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.LockAsync())
            {
                if (this.IsDisposed)
                {
                    return new ConnectorResult(ConnectorResultType.Failed);
                }

                var garbages = new List<IDisposable>();

                try
                {
                    var config = _tcpAcceptOptions;
                    if (config == null || !config.Enabled)
                    {
                        return new ConnectorResult(ConnectorResultType.Failed);
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

                        return new ConnectorResult(ConnectorResultType.Succeeded, new SocketCap(socket, false), address);
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

                return new ConnectorResult(ConnectorResultType.Failed);
            }
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
    }
}
