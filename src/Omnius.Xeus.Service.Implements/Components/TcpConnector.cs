using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Extensions;
using Omnius.Core.Network;
using Omnius.Core.Network.Caps;
using System.Diagnostics;
using Omnius.Xeus.Service.Components;
using Omnius.Core.Configuration;
using Omnius.Xeus.Service;
using Omnius.Xeus.Service.Components.Primitives;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Net.Upnp;

namespace Omnius.Xeus.Engine.Implements.Components
{
    public sealed class TcpConnector : DisposableBase, ITcpConnector
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IBufferPool<byte> _bufferPool;

        private readonly OmniSettings _settings;

        private readonly Random _random = new Random();

        private readonly Task _watchTask;
        private readonly ManualResetEventSlim _watchTaskEvent = new ManualResetEventSlim();

        private readonly Dictionary<OmniAddress, TcpListener> _tcpListenerMap = new Dictionary<OmniAddress, TcpListener>();
        private readonly Dictionary<OmniAddress, ushort> _openedPortsByUpnp = new Dictionary<OmniAddress, ushort>();

        private TcpConnectOptions? _tcpConnectOptions;
        private TcpAcceptOptions? _tcpAcceptOptions;

        private readonly object _lockObject = new object();

        private TcpConnector(string basePath, IBufferPool<byte> bufferPool)
        {
            var configPath = Path.Combine(basePath, "config");
            var refsPath = Path.Combine(basePath, "refs");

            _bufferPool = bufferPool;

            _settings = new OmniSettings(configPath);

            _watchTask = this.Watch();
        }

        private async ValueTask LoadAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    if (_settings.TryGetContent<TcpConnectorConfig>("Config", out var config))
                    {
                        this.SetTcpConnectOptions(config.TcpConnectOptions);
                        this.SetTcpAcceptOptions(config.TcpAcceptOptions);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                    throw e;
                }
            });
        }

        private async ValueTask SaveAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    var config = new TcpConnectorConfig(0, _tcpConnectOptions, _tcpAcceptOptions);
                    _settings.SetContent("Config", config);
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                    throw e;
                }
            });
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _watchTaskEvent.Set();
            await _watchTask;
            _watchTaskEvent.Dispose();

            await this.SaveAsync();
            await _settings.DisposeAsync();
        }

        protected override void OnDispose(bool disposing)
        {
            this.OnDisposeAsync().AsTask().Wait();
        }

        public TcpConnectOptions TcpConnectOptions => _tcpConnectOptions;

        public TcpAcceptOptions TcpAcceptOptions => _tcpAcceptOptions;

        public void SetTcpConnectOptions(TcpConnectOptions tcpConnectConfig)
        {
            lock (_lockObject)
            {
                if (_tcpConnectOptions == tcpConnectConfig)
                {
                    return;
                }

                _tcpConnectOptions = tcpConnectConfig;
            }
        }

        public void SetTcpAcceptOptions(TcpAcceptOptions tcpAcceptConfig)
        {
            lock (_lockObject)
            {
                if (_tcpAcceptOptions == tcpAcceptConfig)
                {
                    return;
                }

                _tcpAcceptOptions = tcpAcceptConfig;
            }
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

        private static async ValueTask<Socket?> ConnectSocketAsync(IPEndPoint remoteEndPoint)
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
                        var socket = await ConnectSocketAsync(new IPEndPoint(proxyAddress, proxyPort));
                        if (socket == null)
                        {
                            return new ConnectorResult(ConnectorResultType.Failed);
                        }

                        disposableList.Add(socket);

                        var proxy = new Socks5ProxyClient(ipAddress.ToString(), port);
                        proxy.Create(socket, cancellationToken);

                        var cap = new SocketCap(socket, false);
                        disposableList.Add(cap);

                        return new ConnectorResult(ConnectorResultType.Succeeded, cap, address);
                    }
                    else if (config.ProxyOptions.Type == TcpProxyType.HttpProxy)
                    {
                        var socket = await ConnectSocketAsync(new IPEndPoint(proxyAddress, proxyPort));
                        if (socket == null)
                        {
                            return new ConnectorResult(ConnectorResultType.Failed);
                        }

                        disposableList.Add(socket);

                        var proxy = new HttpProxyClient(ipAddress.ToString(), port);
                        proxy.Create(socket, cancellationToken);

                        var cap = new SocketCap(socket, false);
                        disposableList.Add(cap);

                        return new ConnectorResult(ConnectorResultType.Succeeded, cap, address);
                    }
                }
                else
                {
                    var socket = await ConnectSocketAsync(new IPEndPoint(ipAddress, port));
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

        public async ValueTask<ConnectorResult> AcceptAsync(CancellationToken cancellationToken = default)
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

                var tcpListeners = _tcpListenerMap.Select(n => n.Value).ToArray();
                _random.Shuffle(tcpListeners);

                foreach (var tcpListener in tcpListeners)
                {
                    var socket = await tcpListener.AcceptSocketAsync();
                    garbages.Add(socket);

                    var endpoint = (IPEndPoint)socket.RemoteEndPoint;

                    OmniAddress address;

                    if (endpoint.AddressFamily == AddressFamily.InterNetwork)
                    {
                        address = new OmniAddress($"/ip4/{endpoint.Address}/tcp/{endpoint.Port}");
                    }
                    else if (endpoint.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        address = new OmniAddress($"/ip6/{endpoint.Address}/tcp/{endpoint.Port}");
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

        private async Task Watch()
        {
            try
            {
                var upnpStopwatch = new Stopwatch();

                while (!_watchTaskEvent.Wait(1000))
                {
                    if (!upnpStopwatch.IsRunning || upnpStopwatch.Elapsed.TotalSeconds > 10)
                    {
                        await this.WatchUpnp();
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private async Task WatchUpnp()
        {
            var config = _tcpAcceptOptions;
            var listenAddressSet = new HashSet<OmniAddress>(config?.ListenAddresses.ToArray() ?? Array.Empty<OmniAddress>());
            var useUpnp = config?.UseUpnp ?? false;

            UpnpClient? upnpClient = null;

            try
            {
                // 不要なTcpListenerの削除処理
                foreach (var (omniAddress, tcpListener) in _tcpListenerMap.ToArray())
                {
                    if (listenAddressSet.Contains(omniAddress))
                    {
                        continue;
                    }

                    tcpListener.Server.Dispose();
                    tcpListener.Stop();

                    _tcpListenerMap.Remove(omniAddress);
                }

                {
                    var unusedPortSet = new HashSet<ushort>();

                    if (!useUpnp)
                    {
                        // UPnPで開放していたポートをすべて閉じる
                        unusedPortSet.UnionWith(_openedPortsByUpnp.Select(n => n.Value).ToArray());
                        _openedPortsByUpnp.Clear();
                    }
                    else
                    {
                        // UPnPで開放していた利用されていないポートを閉じる
                        foreach (var (omniAddress, port) in _openedPortsByUpnp.ToArray())
                        {
                            if (listenAddressSet.Contains(omniAddress))
                            {
                                continue;
                            }

                            unusedPortSet.Add(port);
                            _openedPortsByUpnp.Remove(omniAddress);
                        }
                    }

                    if (unusedPortSet.Count > 0)
                    {
                        try
                        {
                            if (upnpClient == null)
                            {
                                upnpClient = new UpnpClient();
                                await upnpClient.ConnectAsync();
                            }

                            foreach (var port in unusedPortSet)
                            {
                                await upnpClient.ClosePortAsync(UpnpProtocolType.Tcp, port);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e);
                        }
                    }
                }

                // TcpListenerの追加処理
                foreach (var listenAddress in listenAddressSet)
                {
                    if (_tcpListenerMap.ContainsKey(listenAddress))
                    {
                        continue;
                    }

                    if (!TryGetEndpoint(listenAddress, out var ipAddress, out ushort port, false))
                    {
                        continue;
                    }

                    var tcpListener = new TcpListener(ipAddress, port);
                    tcpListener.Start(3);

                    _tcpListenerMap.Add(listenAddress, tcpListener);
                }

                // UPnPでのポート開放
                if (useUpnp)
                {
                    foreach (var listenAddress in listenAddressSet)
                    {
                        if (_openedPortsByUpnp.ContainsKey(listenAddress))
                        {
                            continue;
                        }

                        if (!TryGetEndpoint(listenAddress, out var ipAddress, out ushort port, false))
                        {
                            continue;
                        }

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
