using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Base;
using Omnix.Base.Extensions;
using Omnix.Configuration;
using Omnix.DataStructures;
using Omnix.Net.Upnp;
using Omnix.Network;
using Omnix.Network.Proxies;
using Xeus.Core.Internal.Primitives;
using Xeus.Messages;

namespace Xeus.Core.Internal.Connection.Primitives
{
    internal sealed class TcpConnectionCreator : ServiceBase, ISettings
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly BufferPool _bufferPool;

        private readonly SettingsDatabase _settings;

        private readonly Random _random = new Random();

        private readonly EventScheduler _watchEventScheduler;

        private readonly LockedHashDictionary<OmniAddress, TcpListener> _tcpListenerMap = new LockedHashDictionary<OmniAddress, TcpListener>();
        private readonly LockedHashDictionary<OmniAddress, ushort> _openedPortsByUpnp = new LockedHashDictionary<OmniAddress, ushort>();

        private TcpConnectOptions? _tcpConnectOptions;
        private TcpAcceptOptions? _tcpAcceptOptions;

        private readonly object _lockObject = new object();
        private readonly AsyncLock _settingsAsyncLock = new AsyncLock();

        public TcpConnectionCreator(string basePath, BufferPool bufferPool)
        {
            var settingsPath = Path.Combine(basePath, "Settings");
            var childrenPath = Path.Combine(basePath, "Children");

            _bufferPool = bufferPool;

            _settings = new SettingsDatabase(settingsPath);

            _watchEventScheduler = new EventScheduler(this.WatchThread);
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

            _watchEventScheduler.ExecuteImmediate();
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

            _watchEventScheduler.ExecuteImmediate();
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

        private static bool TryGetEndpoint(OmniAddress omniAddress, out IPAddress ipAddress, out ushort port, bool nameResolving = false)
        {
            ipAddress = IPAddress.None;
            port = 0;

            var sections = omniAddress.Decompose();

            // フォーマットのチェック
            if (sections.Length != 4 || !(sections[0] == "ip4" || sections[0] == "ip6") || !(sections[2] == "tcp"))
            {
                return false;
            }

            // IPアドレスのパース処理
            {
                if (nameResolving)
                {
                    if (!IPAddress.TryParse(sections[1], out ipAddress))
                    {
                        try
                        {
                            var hostEntry = Dns.GetHostEntry(sections[1]);

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
                }
                else
                {
                    if (!IPAddress.TryParse(sections[1], out ipAddress))
                    {
                        return false;
                    }
                }

                if (sections[0] == "ip4" && ipAddress.AddressFamily != AddressFamily.InterNetwork)
                {
                    return false;
                }

                if (sections[0] == "ip6" && ipAddress.AddressFamily != AddressFamily.InterNetworkV6)
                {
                    return false;
                }
            }

            // ポート番号のパース処理
            if (ushort.TryParse(sections[3], out port))
            {
                return false;
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

        public async ValueTask<Cap?> ConnectAsync(OmniAddress address, CancellationToken token = default)
        {
            if (this.IsDisposed)
            {
                return null;
            }

            if (this.StateType != ServiceStateType.Running)
            {
                return null;
            }

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
                        var socket = await ConnectSocketAsync(new IPEndPoint(proxyAddress, proxyPort));
                        if (socket == null)
                        {
                            return null;
                        }

                        disposableList.Add(socket);

                        var proxy = new Socks5ProxyClient(ipAddress.ToString(), port);
                        proxy.Create(socket, token);

                        var cap = new SocketCap(socket, false);
                        disposableList.Add(cap);

                        return cap;
                    }
                    else if (config.ProxyOptions.Type == TcpProxyType.HttpProxy)
                    {
                        var socket = await ConnectSocketAsync(new IPEndPoint(proxyAddress, proxyPort));
                        if (socket == null)
                        {
                            return null;
                        }

                        disposableList.Add(socket);

                        var proxy = new HttpProxyClient(ipAddress.ToString(), port);
                        proxy.Create(socket, token);

                        var cap = new SocketCap(socket, false);
                        disposableList.Add(cap);

                        return cap;
                    }
                }
                else
                {
                    var socket = await ConnectSocketAsync(new IPEndPoint(ipAddress, port));
                    if (socket == null)
                    {
                        return null;
                    }

                    disposableList.Add(socket);

                    var cap = new SocketCap(socket, false);
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

        public async ValueTask<(Cap?, OmniAddress?)> AcceptAsync(CancellationToken token = default)
        {
            if (this.IsDisposed)
            {
                return default;
            }

            if (this.StateType != ServiceStateType.Running)
            {
                return default;
            }

            var garbages = new List<IDisposable>();

            try
            {
                var config = _tcpAcceptOptions;
                if (config == null || !config.Enabled)
                {
                    return default;
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

                    return (new SocketCap(socket, false), address);
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

        private async ValueTask WatchThread(CancellationToken token)
        {
            try
            {
                for (; ; )
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

        protected override async ValueTask OnInitializeAsync()
        {
        }

        protected override async ValueTask OnStartAsync()
        {
            this.StateType = ServiceStateType.Starting;

            _watchEventScheduler.ChangeInterval(new TimeSpan(0, 30, 0));
            await _watchEventScheduler.StartAsync();

            this.StateType = ServiceStateType.Running;
        }

        protected override async ValueTask OnStopAsync()
        {
            this.StateType = ServiceStateType.Stopping;

            await _watchEventScheduler.StopAsync();

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

            this.StateType = ServiceStateType.Stopped;
        }

        public async ValueTask LoadAsync()
        {
            await Task.Run(async () =>
            {
                using (await _settingsAsyncLock.LockAsync())
                {
                    try
                    {
                        if (_settings.TryGetContent<TcpConnectionCreatorConfig>("Config", out var config))
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
                }
            });
        }

        public async ValueTask SaveAsync()
        {
            await Task.Run(async () =>
            {
                using (await _settingsAsyncLock.LockAsync())
                {
                    try
                    {
                        var config = new TcpConnectionCreatorConfig(0, _tcpConnectOptions, _tcpAcceptOptions);
                        _settings.SetContent("Config", config);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e);
                        throw e;
                    }
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.StopAsync().AsTask().Wait();
            }
        }
    }
}
