using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Amoeba.Messages;
using Omnix.Base;
using Omnix.Configuration;
using Omnix.Net;
using Omnix.Net.Proxy;
using Omnix.Utils;

namespace Xeus.Core
{
    partial class ConnectionManager
    {
        public sealed class CustomConnectionManager : StateManagerBase, ISettings
        {
            private BufferPool _bufferPool;
            private CatharsisManager _catharsisManager;

            private Settings _settings;

            private Random _random = new Random();

            private CustomConnectionConfig _config;

            private Dictionary<string, TcpListener> _tcpListeners = new Dictionary<string, TcpListener>();

            private TimerScheduler _watchTimer;

            private List<string> _locationUris = new List<string>();

            private volatile ManagerState _state = ManagerState.Stop;

            private AtomicCounter _blockCount = new AtomicCounter();

            private readonly object _lockObject = new object();
            private volatile bool _disposed;

            public CustomConnectionManager(string configPath, CatharsisManager catharsisManager, BufferPool bufferPool)
            {
                _bufferPool = bufferPool;
                _catharsisManager = catharsisManager;

                _settings = new Settings(configPath);

                _watchTimer = new TimerScheduler(this.WatchListenerThread);
            }

            public CustomConnectionReport Report
            {
                get
                {
                    lock (_lockObject)
                    {
                        return new CustomConnectionReport(_blockCount);
                    }
                }
            }

            public CustomConnectionConfig Config
            {
                get
                {
                    lock (_lockObject)
                    {
                        return _config;
                    }
                }
            }

            public void SetConfig(CustomConnectionConfig config)
            {
                lock (_lockObject)
                {
                    if (_config == config) return;
                    _config = config;
                }

                _watchTimer.RunOnce();
            }

            public IEnumerable<string> LocationUris
            {
                get
                {
                    lock (_lockObject)
                    {
                        return _locationUris.ToArray();
                    }
                }
            }

            private static bool CheckGlobalIpAddress(IPAddress ipAddress)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (IPAddress.Any.ToString() == ipAddress.ToString()
                        || IPAddress.Loopback.ToString() == ipAddress.ToString()
                        || IPAddress.Broadcast.ToString() == ipAddress.ToString())
                    {
                        return false;
                    }
                    if (CollectionUtils.Compare(ipAddress.GetAddressBytes(), IPAddress.Parse("10.0.0.0").GetAddressBytes()) >= 0
                        && CollectionUtils.Compare(ipAddress.GetAddressBytes(), IPAddress.Parse("10.255.255.255").GetAddressBytes()) <= 0)
                    {
                        return false;
                    }
                    if (CollectionUtils.Compare(ipAddress.GetAddressBytes(), IPAddress.Parse("172.16.0.0").GetAddressBytes()) >= 0
                        && CollectionUtils.Compare(ipAddress.GetAddressBytes(), IPAddress.Parse("172.31.255.255").GetAddressBytes()) <= 0)
                    {
                        return false;
                    }
                    if (CollectionUtils.Compare(ipAddress.GetAddressBytes(), IPAddress.Parse("127.0.0.0").GetAddressBytes()) >= 0
                        && CollectionUtils.Compare(ipAddress.GetAddressBytes(), IPAddress.Parse("127.255.255.255").GetAddressBytes()) <= 0)
                    {
                        return false;
                    }
                    if (CollectionUtils.Compare(ipAddress.GetAddressBytes(), IPAddress.Parse("192.168.0.0").GetAddressBytes()) >= 0
                        && CollectionUtils.Compare(ipAddress.GetAddressBytes(), IPAddress.Parse("192.168.255.255").GetAddressBytes()) <= 0)
                    {
                        return false;
                    }
                }
                if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    if (IPAddress.IPv6Any.ToString() == ipAddress.ToString()
                        || IPAddress.IPv6Loopback.ToString() == ipAddress.ToString()
                        || IPAddress.IPv6None.ToString() == ipAddress.ToString())
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
                    list.UnionWith(Dns.GetHostAddressesAsync(Dns.GetHostName()).Result.Where(n => CheckGlobalIpAddress(n)));
                }
                catch (Exception)
                {

                }

                return list;
            }

            private static IPAddress GetIpAddress(string host)
            {
                IPAddress remoteIp;

                if (!IPAddress.TryParse(host, out remoteIp))
                {
                    var hostEntry = Dns.GetHostEntryAsync(host).Result;

                    if (hostEntry.AddressList.Length > 0)
                    {
                        remoteIp = hostEntry.AddressList[0];
                    }
                    else
                    {
                        return null;
                    }
                }

                return remoteIp;
            }

            private static Socket Connect(IPEndPoint remoteEndPoint)
            {
                Socket socket = null;

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
                    if (socket != null) socket.Dispose();

                    throw;
                }
            }

            public Cap ConnectCap(string uri)
            {
                if (_disposed) return null;
                if (this.State == ManagerState.Stop) return null;

                var garbages = new List<IDisposable>();

                try
                {
                    var config = this.Config;

                    var result = UriUtils.Parse(uri);
                    if (result == null) throw new Exception();

                    string scheme = result.GetValue<string>("Scheme");
                    string address = result.GetValue<string>("Address");
                    int port = result.GetValueOrDefault<int>("Port", () => 4050);

                    var connectionFilter = config.ConnectionFilters.FirstOrDefault(n => n.Scheme == scheme);
                    if (connectionFilter == null || connectionFilter.Type == ConnectionType.None) return null;

                    if (connectionFilter.Type == ConnectionType.Tcp)
                    {
                        // Check
                        {
                            IPAddress ipAddress;

                            if (!IPAddress.TryParse(address, out ipAddress)) return null;

#if !DEBUG
                            if (!CheckGlobalIpAddress(ipAddress)) return null;
#endif

                            if (!_catharsisManager.Check(ipAddress))
                            {
                                _blockCount.Increment();

                                return null;
                            }
                        }

                        var socket = Connect(new IPEndPoint(IPAddress.Parse(address), port));
                        garbages.Add(socket);

                        var cap = new SocketCap(socket);
                        garbages.Add(cap);

                        return cap;
                    }
                    else if (connectionFilter.Type == ConnectionType.Socks5Proxy
                        || connectionFilter.Type == ConnectionType.HttpProxy)
                    {
                        var result2 = UriUtils.Parse(connectionFilter.ProxyUri);
                        if (result2 == null) throw new Exception();

                        string proxyScheme = result2.GetValue<string>("Scheme");
                        if (proxyScheme != "tcp") throw new Exception();

                        if (connectionFilter.Type == ConnectionType.HttpProxy)
                        {
                            string proxyAddress = result2.GetValue<string>("Address");
                            int proxyPort = result2.GetValueOrDefault<int>("Port", () => 1080);

                            var socket = Connect(new IPEndPoint(GetIpAddress(proxyAddress), proxyPort));
                            garbages.Add(socket);

                            var proxy = new HttpProxyClient(address, port);
                            proxy.Create(socket, new TimeSpan(0, 0, 30));

                            var cap = new SocketCap(socket);
                            garbages.Add(cap);

                            return cap;
                        }
                        else if (connectionFilter.Type == ConnectionType.Socks5Proxy)
                        {
                            string proxyAddress = result2.GetValue<string>("Address");
                            int proxyPort = result2.GetValueOrDefault<int>("Port", () => 80);

                            var socket = Connect(new IPEndPoint(GetIpAddress(proxyAddress), proxyPort));
                            garbages.Add(socket);

                            var proxy = new Socks5ProxyClient(address, port);
                            proxy.Create(socket, new TimeSpan(0, 0, 30));

                            var cap = new SocketCap(socket);
                            garbages.Add(cap);

                            return cap;
                        }
                    }
                }
                catch (Exception)
                {
                    foreach (var item in garbages)
                    {
                        item.Dispose();
                    }
                }

                return null;
            }

            public Cap AcceptCap(out string uri)
            {
                uri = null;

                if (_disposed) return null;
                if (this.State == ManagerState.Stop) return null;

                var garbages = new List<IDisposable>();

                try
                {
                    var config = this.Config;

                    foreach (var tcpListener in _tcpListeners.Values)
                    {
                        if (!tcpListener.Pending()) continue;

                        var socket = tcpListener.AcceptSocketAsync().Result;
                        garbages.Add(socket);

                        // Check
                        {
                            var ipEndPoint = ((IPEndPoint)socket.RemoteEndPoint);
                            if (!_catharsisManager.Check(ipEndPoint.Address)) return null;

                            uri = $"tcp:{ipEndPoint.Address}:{ipEndPoint.Port}";
                        }

                        return new SocketCap(socket);
                    }
                }
                catch (Exception)
                {
                    foreach (var item in garbages)
                    {
                        item.Dispose();
                    }
                }

                return null;
            }

            private void WatchListenerThread()
            {
                try
                {
                    for (; ; )
                    {
                        var config = this.Config;

                        foreach (var (uri, tcpListener) in _tcpListeners.ToArray())
                        {
                            if (config.ListenUris.Contains(uri)) continue;

                            tcpListener.Stop();
                            _tcpListeners.Remove(uri);
                        }

                        foreach (string uri in config.ListenUris)
                        {
                            if (_tcpListeners.ContainsKey(uri)) continue;

                            var result = UriUtils.Parse(uri);
                            if (result == null) throw new Exception();

                            string scheme = result.GetValue<string>("Scheme");
                            string address = result.GetValue<string>("Address");
                            int port = result.GetValueOrDefault<int>("Port", () => 4050);

                            if (scheme == "tcp")
                            {
                                try
                                {
                                    var listener = new TcpListener(IPAddress.Parse(address), port);
                                    listener.Start(3);
                                    _tcpListeners[uri] = listener;
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }

                        lock (_lockObject)
                        {
                            if (this.Config != config) continue;

                            _locationUris.Clear();
                            _locationUris.AddRange(config.LocationUris);
                        }

                        return;
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            public override ManagerState State
            {
                get
                {
                    return _state;
                }
            }

            private readonly object _stateLockObject = new object();

            public override void Start()
            {
                lock (_stateLockObject)
                {
                    lock (_lockObject)
                    {
                        if (this.State == ManagerState.Start) return;
                        _state = ManagerState.Start;

                        _watchTimer.Start(new TimeSpan(0, 0, 0), new TimeSpan(0, 30, 0));
                    }
                }
            }

            public override void Stop()
            {
                lock (_stateLockObject)
                {
                    lock (_lockObject)
                    {
                        if (this.State == ManagerState.Stop) return;
                        _state = ManagerState.Stop;
                    }

                    _watchTimer.Stop();

                    foreach (var tcpListener in _tcpListeners.Values)
                    {
                        tcpListener.Stop();
                    }
                    _tcpListeners.Clear();
                }
            }

            #region ISettings

            public void Load()
            {
                lock (_lockObject)
                {
                    int version = _settings.Load("Version", () => 0);

                    _config = _settings.Load<CustomConnectionConfig>("Config", () =>
                    {
                        var connectionFilters = new List<ConnectionFilter>();
                        connectionFilters.Add(new ConnectionFilter("tcp", ConnectionType.None, "tcp:127.0.0.1:19050"));
                        connectionFilters.Add(new ConnectionFilter("tor", ConnectionType.Socks5Proxy, "tcp:127.0.0.1:19050"));

                        var listenUris = new List<string>();
                        listenUris.Add($"tcp:{IPAddress.Loopback}:4050");
                        listenUris.Add($"tcp:[{IPAddress.IPv6Loopback}]:4050");

                        return new CustomConnectionConfig(Array.Empty<string>(), connectionFilters, listenUris);
                    });
                }
            }

            public void Save()
            {
                lock (_lockObject)
                {
                    _settings.Save("Version", 0);

                    _settings.Save("Config", _config);
                }
            }

            #endregion

            protected override void Dispose(bool disposing)
            {
                if (_disposed) return;
                _disposed = true;

                if (disposing)
                {

                }
            }
        }
    }
}
