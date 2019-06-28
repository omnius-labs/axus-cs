using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Base;
using Omnix.Base.Extensions;
using Omnix.Collections;
using Omnix.Configuration;
using Omnix.Net.Upnp;
using Omnix.Network;
using Omnix.Network.Proxy;
using Xeus.Core.Primitives;
using Xeus.Messages;

namespace Xeus.Core.Connections.Internal
{
    public sealed class TcpConnectionCreator : ServiceBase, ISettings
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly BufferPool _bufferPool;

        private readonly SettingsDatabase _settings;

        private readonly Random _random = new Random();

        private readonly EventScheduler _watchEventScheduler;

        private readonly LockedHashDictionary<OmniAddress, TcpListener> _tcpListenerMap = new LockedHashDictionary<OmniAddress, TcpListener>();
        private readonly LockedHashDictionary<OmniAddress, ushort> _openedPortsByUpnp = new LockedHashDictionary<OmniAddress, ushort>();

        private readonly LockedList<ushort> _lastOpenedPortsByUpnp = new LockedList<ushort>();

        private TcpConnectConfig? _tcpConnectConfig;
        private TcpAcceptConfig? _tcpAcceptConfig;

        private ServiceStateType _stateType = ServiceStateType.Stopped;

        private readonly AsyncLock _asyncLock = new AsyncLock();
        private readonly object _lockObject = new object();
        private volatile bool _disposed;

        public TcpConnectionCreator(string configPath, BufferPool bufferPool)
        {
            _bufferPool = bufferPool;

            _settings = new SettingsDatabase(configPath);

            _watchEventScheduler = new EventScheduler(this.WatchThread);
        }

        public TcpConnectConfig? TcpConnectConfig => _tcpConnectConfig;

        public TcpAcceptConfig? TcpAcceptConfig => _tcpAcceptConfig;

        public void SetTcpConnectConfig(TcpConnectConfig? tcpConnectConfig)
        {
            lock (_lockObject)
            {
                if (_tcpConnectConfig == tcpConnectConfig)
                {
                    return;
                }

                _tcpConnectConfig = tcpConnectConfig;
            }

            _watchEventScheduler.ExecuteImmediate();
        }

        public void SetTcpAcceptConfig(TcpAcceptConfig? tcpAcceptConfig)
        {
            lock (_lockObject)
            {
                if (_tcpAcceptConfig == tcpAcceptConfig)
                {
                    return;
                }

                _tcpAcceptConfig = tcpAcceptConfig;
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

            var sections = omniAddress.Parse();

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

        private static Socket? CreateSocket(IPEndPoint remoteEndPoint)
        {
            Socket? socket = null;

            try
            {
                socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.SendTimeout = 1000 * 10;
                socket.ReceiveTimeout = 1000 * 10;
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
        }

        public Cap? Connect(OmniAddress address, CancellationToken token = default)
        {
            if (_disposed)
            {
                return null;
            }

            if (this.StateType != ServiceStateType.Running)
            {
                return null;
            }

            var config = _tcpConnectConfig;
            if (config == null || !config.Enabled)
            {
                return null;
            }

            IPAddress ipAddress;
            ushort port;

            if (!TryGetEndpoint(address, out ipAddress, out port))
            {
                return null;
            }

            var disposableList = new List<IDisposable>();

            try
            {
#if !DEBUG
                if (!IsGlobalIpAddress(ipAddress)) return null;
#endif

                if (config.ProxyConfig != null)
                {
                    IPAddress proxyAddress;
                    ushort proxyPort;

                    if (!TryGetEndpoint(config.ProxyConfig.Address, out proxyAddress, out proxyPort, true))
                    {
                        return null;
                    }

                    if (config.ProxyConfig.Type == TcpProxyType.Socks5Proxy)
                    {
                        var socket = CreateSocket(new IPEndPoint(proxyAddress, proxyPort));
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
                    else if (config.ProxyConfig.Type == TcpProxyType.HttpProxy)
                    {
                        var socket = CreateSocket(new IPEndPoint(proxyAddress, proxyPort));
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
                    var socket = CreateSocket(new IPEndPoint(ipAddress, port));
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

        public Cap? Accept(out OmniAddress? address, CancellationToken token = default)
        {
            address = null;

            if (_disposed)
            {
                return null;
            }

            if (this.StateType != ServiceStateType.Running)
            {
                return null;
            }

            var garbages = new List<IDisposable>();

            try
            {
                var config = _tcpAcceptConfig;
                if (config == null || !config.Enabled)
                {
                    return null;
                }

                foreach (var tcpListener in _tcpListenerMap.ToArray().Randomize().Select(n => n.Value))
                {
                    var socket = tcpListener.AcceptSocket();
                    garbages.Add(socket);

                    var endpoint = (IPEndPoint)socket.RemoteEndPoint;

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

                    return new SocketCap(socket, false);
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

            return null;
        }

        private async Task WatchThread(CancellationToken token)
        {
            try
            {
                for (; ; )
                {
                    var config = _tcpAcceptConfig;
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

                        // UPnPで開放していた利用されていないポートを閉じる
                        foreach (var (omniAddress, port) in _openedPortsByUpnp.ToArray())
                        {
                            if (listenAddressSet.Contains(omniAddress))
                            {
                                continue;
                            }

                            if (upnpClient == null)
                            {
                                upnpClient = new UpnpClient();
                                await upnpClient.ConnectAsync(token);
                            }

                            await upnpClient.ClosePortAsync(UpnpProtocolType.Tcp, port, token);
                        }

                        // TcpListenerの追加処理
                        foreach (var listenAddress in listenAddressSet)
                        {
                            if (_tcpListenerMap.ContainsKey(listenAddress))
                            {
                                continue;
                            }

                            IPAddress ipAddress;
                            ushort port;

                            if (!TryGetEndpoint(listenAddress, out ipAddress, out port, false))
                            {
                                continue;
                            }

                            var tcpListener = new TcpListener(IPAddress.Any, port);
                            tcpListener.Start(3);

                            _tcpListenerMap.Add(listenAddress, tcpListener);
                        }

                        // TcpListenerの追加処理
                        foreach (var listenAddress in listenAddressSet)
                        {
                            if (_openedPortsByUpnp.ContainsKey(listenAddress))
                            {
                                continue;
                            }

                            IPAddress ipAddress;
                            ushort port;

                            if (!TryGetEndpoint(listenAddress, out ipAddress, out port, false))
                            {
                                continue;
                            }

                            // "0.0.0.0"以外は対象外
                            if (ipAddress != IPAddress.Any)
                            {
                                continue;
                            }

                            if (upnpClient == null)
                            {
                                upnpClient = new UpnpClient();
                                await upnpClient.ConnectAsync(token);
                            }

                            await upnpClient.OpenPortAsync(UpnpProtocolType.Tcp, port, port, "Xeus", token);
                        }
                    }
                    finally
                    {
                        if(upnpClient != null)
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

        public override ServiceStateType StateType { get; }

        private readonly object _stateLockObject = new object();

        private async ValueTask InternalStart()
        {
            _stateType = ServiceStateType.Starting;

            _watchEventScheduler.ChangeInterval(new TimeSpan(0, 30, 0));
            await _watchEventScheduler.StartAsync();

            _stateType = ServiceStateType.Running;
        }

        private async ValueTask InternalStop()
        {
            _stateType = ServiceStateType.Stopping;

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

            _stateType = ServiceStateType.Stopped;
        }

        public override async ValueTask StartAsync()
        {
            using (await _asyncLock.LockAsync())
            {
                if (this.StateType != ServiceStateType.Stopped)
                {
                    return;
                }

                await this.InternalStart();
            }
        }

        public override async ValueTask StopAsync()
        {
            using (await _asyncLock.LockAsync())
            {
                if (this.StateType != ServiceStateType.Running)
                {
                    return;
                }

                await this.InternalStop();
            }
        }

        public override async ValueTask RestartAsync()
        {
            using (await _asyncLock.LockAsync())
            {
                if (this.StateType != ServiceStateType.Stopped)
                {
                    await this.InternalStop();
                }

                await this.InternalStart();
            }
        }

        public async ValueTask LoadAsync()
        {
            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    try
                    {
                        if (_settings.TryGetContent<TcpConnectionCreatorConfig>("Config", out var config))
                        {
                            this.SetTcpConnectConfig(config.TcpConnectConfig);
                            this.SetTcpAcceptConfig(config.TcpAcceptConfig);

                            lock (_lastOpenedPortsByUpnp.LockObject)
                            {
                                _lastOpenedPortsByUpnp.Clear();
                                _lastOpenedPortsByUpnp.AddRange(config.OpenedPortsByUpnp);
                            }
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
            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    try
                    {
                        var config = new TcpConnectionCreatorConfig(0, _tcpConnectConfig, _tcpAcceptConfig, _openedPortsByUpnp.Values.ToArray());
                        _settings.SetContent("Config", config);
                        _settings.Commit();
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e);

                        _settings.Rollback();

                        throw e;
                    }
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (disposing)
            {
                this.StopAsync().AsTask().Wait();
            }
        }
    }
}
