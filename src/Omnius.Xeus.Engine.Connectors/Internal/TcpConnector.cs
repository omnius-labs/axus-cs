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
using Omnius.Core.Configuration;
using Omnius.Core.Collections;
using Omnius.Core.Net.Upnp;
using Omnius.Core.Network;
using Omnius.Core.Network.Caps;
using Omnius.Core.Network.Proxies;

namespace Xeus.Engine.Connectors.Internal
{
    internal sealed class TcpConnector : ServiceBase, ITcpConnector
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IBufferPool<byte> _bufferPool;

        private readonly OmniSettings _settings;

        private readonly Random _random = new Random();

        private readonly EventTimer _watchEventTimer;

        private readonly LockedHashDictionary<OmniAddress, TcpListener> _tcpListenerMap = new LockedHashDictionary<OmniAddress, TcpListener>();
        private readonly LockedHashDictionary<OmniAddress, ushort> _openedPortsByUpnp = new LockedHashDictionary<OmniAddress, ushort>();

        private TcpConnectOptions? _tcpConnectOptions;
        private TcpAcceptOptions? _tcpAcceptOptions;

        private readonly object _lockObject = new object();
        private readonly AsyncLock _settingsAsyncLock = new AsyncLock();

        public TcpConnector(string basePath, IBufferPool<byte> bufferPool)
        {
            var configPath = Path.Combine(basePath, "config");
            var refsPath = Path.Combine(basePath, "refs");

            _bufferPool = bufferPool;

            _settings = new OmniSettings(configPath);

            _watchEventTimer = new EventTimer(this.WatchThread);
        }

        public TcpConnectOptions? TcpConnectOptions => _tcpConnectOptions;

        public TcpAcceptOptions? TcpAcceptOptions => _tcpAcceptOptions;

        public void SetTcpConnectOptions(TcpConnectOptions? tcpConnectConfig)
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

        public void SetTcpAcceptOptions(TcpAcceptOptions? tcpAcceptConfig)
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

        public async ValueTask<ConnectorResult> ConnectAsync(OmniAddress address, CancellationToken token = default)
        {
            if (this.IsDisposed)
            {
                return new ConnectorResult(ConnectorResultType.Failed);
            }

            if (this.StateType != ServiceStateType.Running)
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
                        proxy.Create(socket, token);

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
                        proxy.Create(socket, token);

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

        public async ValueTask<ConnectorResult> AcceptAsync(CancellationToken token = default)
        {
            if (this.IsDisposed)
            {
                return new ConnectorResult(ConnectorResultType.Failed);
            }

            if (this.StateType != ServiceStateType.Running)
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

                foreach (var tcpListener in _tcpListenerMap.ToArray().Randomize().Select(n => n.Value))
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

        private async ValueTask WatchThread(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
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
                                        await upnpClient.ConnectAsync(token);
                                    }

                                    foreach (var port in unusedPortSet)
                                    {
                                        await upnpClient.ClosePortAsync(UpnpProtocolType.Tcp, port, token);
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
                                        await upnpClient.ConnectAsync(token);
                                    }

                                    await upnpClient.OpenPortAsync(UpnpProtocolType.Tcp, port, port, "Xeus", token);
                                }
                            }
                        }
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
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private async ValueTask LoadAsync()
        {
            using (await _settingsAsyncLock.LockAsync())
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
        }

        private async ValueTask SaveAsync()
        {
            using (await _settingsAsyncLock.LockAsync())
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
        }

        protected override async ValueTask OnInitializeAsync()
        {
            await this.LoadAsync();
        }

        protected override async ValueTask OnStartAsync()
        {
            this.StateType = ServiceStateType.Starting;

            _watchEventTimer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5));
            await _watchEventTimer.StartAsync();

            this.StateType = ServiceStateType.Running;
        }

        protected override async ValueTask OnStopAsync()
        {
            this.StateType = ServiceStateType.Stopping;

            await _watchEventTimer.StopAsync();

            UpnpClient? upnpClient = null;

            try
            {
                // 不要なTcpListenerの削除処理
                foreach (var (omniAddress, tcpListener) in _tcpListenerMap.ToArray())
                {
                    tcpListener.Server.Dispose();
                    tcpListener.Stop();

                    _tcpListenerMap.Remove(omniAddress);
                }

                // UPnPで開放していた利用されていないポートを閉じる
                foreach (var (omniAddress, port) in _openedPortsByUpnp.ToArray())
                {
                    if (upnpClient == null)
                    {
                        upnpClient = new UpnpClient();
                        await upnpClient.ConnectAsync();
                    }

                    await upnpClient.ClosePortAsync(UpnpProtocolType.Tcp, port);
                }
            }
            finally
            {
                if (upnpClient != null)
                {
                    upnpClient.Dispose();
                }
            }

            await this.SaveAsync();

            this.StateType = ServiceStateType.Stopped;
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                this.StopAsync().AsTask().Wait();
            }
        }
    }
}
