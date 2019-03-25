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
using Omnix.Net.Upnp;
using Omnix.Utils;

namespace Xeus.Core
{
    partial class ConnectionManager
    {
        public sealed class TcpConnectionManager : StateManagerBase, ISettings
        {
            private BufferPool _bufferPool;
            private CatharsisManager _catharsisManager;

            private Settings _settings;

            private Random _random = new Random();

            private TcpConnectionConfig _config;

            private TcpListener _ipv4TcpListener;
            private TcpListener _ipv6TcpListener;

            private TimerScheduler _watchTimer;

            private List<string> _locationUris = new List<string>();

            private volatile ManagerState _state = ManagerState.Stop;

            private AtomicCounter _blockCount = new AtomicCounter();

            private readonly object _lockObject = new object();
            private volatile bool _disposed;

            public TcpConnectionManager(string configPath, CatharsisManager catharsisManager, BufferPool bufferPool)
            {
                _bufferPool = bufferPool;C:\Local\Projects\OmniusLabs\Xeus\src\Xeus.Core\Exchange\Classes\UnicastClue.cs
                _catharsisManager = catharsisManager;

                _settings = new Settings(configPath);

                _watchTimer = new TimerScheduler(this.WatchListenerThread);
            }

            public TcpConnectionReport Report
            {
                get
                {
                    lock (_lockObject)
                    {
                        return new TcpConnectionReport(_blockCount);
                    }
                }
            }

            public TcpConnectionConfig Config
            {
                get
                {
                    lock (_lockObject)
                    {
                        return _config;
                    }
                }
            }

            public void SetConfig(TcpConnectionConfig config)
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

            private static bool IsGlobalIpAddress(IPAddress ipAddress)
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
                    list.UnionWith(Dns.GetHostAddressesAsync(Dns.GetHostName()).Result.Where(n => IsGlobalIpAddress(n)));
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

                if (!uri.StartsWith("tcp:")) return null;

                var garbages = new List<IDisposable>();

                try
                {
                    var config = this.Config;

                    var result = UriUtils.Parse(uri);
                    if (result == null) throw new Exception();

                    string scheme = result.GetValue<string>("Scheme");
                    if (scheme != "tcp") return null;

                    string address = result.GetValue<string>("Address");
                    int port = result.GetValueOrDefault<int>("Port", () => 4050);

                    // Check
                    {
                        IPAddress ipAddress;

                        if (!IPAddress.TryParse(address, out ipAddress)) return null;

#if !DEBUG
                        if (!IsGlobalIpAddress(ipAddress)) return null;
#endif

                        if (!config.Type.HasFlag(TcpConnectionType.Ipv4)
                            && ipAddress.AddressFamily == AddressFamily.InterNetwork) return null;
                        if (!config.Type.HasFlag(TcpConnectionType.Ipv6)
                            && ipAddress.AddressFamily == AddressFamily.InterNetworkV6) return null;

                        if (!_catharsisManager.Check(ipAddress))
                        {
                            _blockCount.Increment();

                            return null;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(config.ProxyUri))
                    {
                        var result2 = UriUtils.Parse(config.ProxyUri);
                        if (result2 == null) throw new Exception();

                        string proxyScheme = result2.GetValue<string>("Scheme");

                        if (proxyScheme == "socks" || proxyScheme == "socks5")
                        {
                            string proxyAddress = result2.GetValue<string>("Address");
                            int proxyPort = result2.GetValueOrDefault<int>("Port", () => 1080);

                            var socket = Connect(new IPEndPoint(GetIpAddress(proxyAddress), proxyPort));
                            garbages.Add(socket);

                            var proxy = new Socks5ProxyClient(address, port);
                            proxy.Create(socket, new TimeSpan(0, 0, 30));

                            var cap = new SocketCap(socket);
                            garbages.Add(cap);

                            return cap;
                        }
                        else if (proxyScheme == "http")
                        {
                            string proxyAddress = result2.GetValue<string>("Address");
                            int proxyPort = result2.GetValueOrDefault<int>("Port", () => 80);

                            var socket = Connect(new IPEndPoint(GetIpAddress(proxyAddress), proxyPort));
                            garbages.Add(socket);

                            var proxy = new HttpProxyClient(address, port);
                            proxy.Create(socket, new TimeSpan(0, 0, 30));

                            var cap = new SocketCap(socket);
                            garbages.Add(cap);

                            return cap;
                        }
                    }
                    else
                    {
                        var socket = Connect(new IPEndPoint(IPAddress.Parse(address), port));
                        garbages.Add(socket);

                        var cap = new SocketCap(socket);
                        garbages.Add(cap);

                        return cap;
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

                    foreach (int p in new int[] { 0, 1 }.Randomize())
                    {
                        if (p == 0 && config.Type.HasFlag(TcpConnectionType.Ipv4) && _ipv4TcpListener != null && _ipv4TcpListener.Pending())
                        {
                            var socket = _ipv4TcpListener.AcceptSocketAsync().Result;
                            garbages.Add(socket);

                            // Check
                            {
                                var ipEndPoint = ((IPEndPoint)socket.RemoteEndPoint);
                                if (!_catharsisManager.Check(ipEndPoint.Address)) return null;

                                uri = $"tcp:{ipEndPoint.Address}:{ipEndPoint.Port}";
                            }

                            return new SocketCap(socket);
                        }

                        if (p == 1 && config.Type.HasFlag(TcpConnectionType.Ipv6) && _ipv6TcpListener != null && _ipv6TcpListener.Pending())
                        {
                            var socket = _ipv6TcpListener.AcceptSocketAsync().Result;
                            garbages.Add(socket);

                            // Check
                            {
                                var ipEndPoint = ((IPEndPoint)socket.RemoteEndPoint);
                                if (!_catharsisManager.Check(ipEndPoint.Address)) return null;

                                uri = $"tcp:[{ipEndPoint.Address}]:{ipEndPoint.Port}";
                            }

                            return new SocketCap(socket);
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

            private int _watchIpv4Port = -1;
            private int _watchIpv6Port = -1;

            private void WatchListenerThread()
            {
                try
                {
                    for (; ; )
                    {
                        var config = this.Config;

                        string ipv4Uri = null;
                        string ipv6Uri = null;

                        if (config.Type.HasFlag(TcpConnectionType.Ipv4) && config.Ipv4Port != 0)
                        {
                            UpnpClient upnpClient = null;

                            try
                            {
                                {
                                    var ipAddress = GetMyGlobalIpAddresses().FirstOrDefault(n => n.AddressFamily == AddressFamily.InterNetwork);

                                    if (ipAddress != null)
                                    {
                                        ipv4Uri = string.Format("tcp:{0}:{1}", ipAddress.ToString(), config.Ipv4Port);
                                    }
                                }

                                if (ipv4Uri == null)
                                {
                                    upnpClient = new UpnpClient();
                                    upnpClient.Connect(new TimeSpan(0, 0, 30));

                                    if (upnpClient.IsConnected)
                                    {
                                        var ipAddress = IPAddress.Parse(upnpClient.GetExternalIpAddress(new TimeSpan(0, 0, 10)));

                                        if (ipAddress != null && IsGlobalIpAddress(ipAddress))
                                        {
                                            ipv4Uri = string.Format("tcp:{0}:{1}", ipAddress.ToString(), config.Ipv4Port);
                                        }
                                    }
                                }

                                if (_ipv4TcpListener == null || _watchIpv4Port != config.Ipv4Port)
                                {
                                    try
                                    {
                                        if (_ipv4TcpListener != null)
                                        {
                                            _ipv4TcpListener.Server.Dispose();
                                            _ipv4TcpListener.Stop();

                                            _ipv4TcpListener = null;
                                        }

                                        _ipv4TcpListener = new TcpListener(IPAddress.Any, config.Ipv4Port);
                                        _ipv4TcpListener.Start(3);
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Error(e);
                                    }

                                    // Open port
                                    if (upnpClient != null && upnpClient.IsConnected)
                                    {
                                        if (_watchIpv4Port != -1)
                                        {
                                            upnpClient.ClosePort(UpnpProtocolType.Tcp, _watchIpv4Port, new TimeSpan(0, 0, 10));
                                        }

                                        upnpClient.OpenPort(UpnpProtocolType.Tcp, config.Ipv4Port, config.Ipv4Port, "Amoeba", new TimeSpan(0, 0, 10));
                                    }

                                    _watchIpv4Port = config.Ipv4Port;
                                }
                            }
                            finally
                            {
                                if (upnpClient != null)
                                {
                                    upnpClient.Dispose();
                                    upnpClient = null;
                                }
                            }
                        }
                        else
                        {
                            if (_ipv4TcpListener != null)
                            {
                                try
                                {
                                    _ipv4TcpListener.Server.Dispose();
                                    _ipv4TcpListener.Stop();

                                    _ipv4TcpListener = null;
                                }
                                catch (Exception e)
                                {
                                    Log.Error(e);
                                }

                                // Close port
                                try
                                {
                                    using (var client = new UpnpClient())
                                    {
                                        client.Connect(new TimeSpan(0, 0, 10));

                                        client.ClosePort(UpnpProtocolType.Tcp, _watchIpv4Port, new TimeSpan(0, 0, 10));
                                    }
                                }
                                catch (Exception)
                                {

                                }

                                _watchIpv4Port = -1;
                            }
                        }

                        if (config.Type.HasFlag(TcpConnectionType.Ipv6) && config.Ipv6Port != 0)
                        {
                            {
                                var ipAddress = GetMyGlobalIpAddresses().FirstOrDefault(n => n.AddressFamily == AddressFamily.InterNetworkV6);

                                if (ipAddress != null)
                                {
                                    ipv6Uri = string.Format("tcp:[{0}]:{1}", ipAddress.ToString(), config.Ipv6Port);
                                }
                            }

                            if (_ipv6TcpListener == null || _watchIpv6Port != config.Ipv6Port)
                            {
                                try
                                {
                                    if (_ipv6TcpListener != null)
                                    {
                                        _ipv6TcpListener.Server.Dispose();
                                        _ipv6TcpListener.Stop();

                                        _ipv6TcpListener = null;
                                    }

                                    _ipv6TcpListener = new TcpListener(IPAddress.IPv6Any, config.Ipv6Port);
                                    _ipv6TcpListener.Start(3);
                                }
                                catch (Exception e)
                                {
                                    Log.Error(e);
                                }

                                _watchIpv6Port = config.Ipv6Port;
                            }
                        }
                        else
                        {
                            if (_ipv6TcpListener != null)
                            {
                                try
                                {
                                    _ipv6TcpListener.Server.Dispose();
                                    _ipv6TcpListener.Stop();

                                    _ipv6TcpListener = null;
                                }
                                catch (Exception e)
                                {
                                    Log.Error(e);
                                }

                                _watchIpv6Port = -1;
                            }
                        }

                        lock (_lockObject)
                        {
                            if (this.Config != config) continue;

                            _locationUris.Clear();
                            if (ipv4Uri != null) _locationUris.Add(ipv4Uri);
                            if (ipv6Uri != null) _locationUris.Add(ipv6Uri);
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

                        _watchTimer.Start(new TimeSpan(0, 0, 0), new TimeSpan(1, 0, 0, 0));
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

                    if (_ipv4TcpListener != null)
                    {
                        try
                        {
                            _ipv4TcpListener.Server.Dispose();
                            _ipv4TcpListener.Stop();

                            _ipv4TcpListener = null;
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                        }

                        // Close port
                        try
                        {
                            using (var client = new UpnpClient())
                            {
                                client.Connect(new TimeSpan(0, 0, 10));

                                client.ClosePort(UpnpProtocolType.Tcp, _watchIpv4Port, new TimeSpan(0, 0, 10));
                            }
                        }
                        catch (Exception)
                        {

                        }

                        _watchIpv4Port = -1;
                    }

                    if (_ipv6TcpListener != null)
                    {
                        try
                        {
                            _ipv6TcpListener.Server.Dispose();
                            _ipv6TcpListener.Stop();

                            _ipv6TcpListener = null;
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                        }

                        _watchIpv6Port = -1;
                    }
                }
            }

            #region ISettings

            public void Load()
            {
                lock (_lockObject)
                {
                    int version = _settings.Load("Version", () => 0);

                    _config = _settings.Load<TcpConnectionConfig>("Config",
                        () => new TcpConnectionConfig(TcpConnectionType.Ipv4 | TcpConnectionType.Ipv6,
                        (ushort)_random.Next(1024, 50000), (ushort)_random.Next(1024, 50000), null));
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
