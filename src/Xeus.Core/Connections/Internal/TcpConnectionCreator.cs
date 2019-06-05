using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Base;
using Omnix.Base.Extensions;
using Omnix.Network;
using Omnix.Network.Proxy;
using Xeus.Core.Internal;
using Xeus.Messages;
using Xeus.Rpc.Primitives;

namespace Xeus.Core.Connections.Internal
{
    public sealed class TcpConnectionCreator : ServiceBase, ISettings
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly BufferPool _bufferPool;

        private readonly Random _random = new Random();

        private readonly List<TcpListener> _tcpListeners = new List<TcpListener>();

        private readonly EventScheduler _watchTimer;

        private readonly List<OmniAddress> _myAddresses = new List<OmniAddress>();
        private readonly Dictionary<OmniAddress, ushort> OpenedPortsByUpnp = new Dictionary<OmniAddress, ushort>();

        private TcpConnectConfig? _tcpConnectConfig;
        private TcpAcceptConfig? _tcpAcceptConfig;

        private volatile ServiceStateType _stateType = ServiceStateType.Stopped;

        private readonly object _lockObject = new object();
        private volatile bool _disposed;

        public TcpConnectionCreator(BufferPool bufferPool)
        {
            _bufferPool = bufferPool;

            _watchTimer = new EventScheduler(this.WatchListenerThread);
        }

        public TcpConnectConfig? ConnectConfig => _tcpConnectConfig;

        public TcpAcceptConfig? AcceptConfig => _tcpAcceptConfig;

        public void SetConnectConfig(TcpConnectConfig tcpConnectConfig)
        {
            lock (_lockObject)
            {
                if (_tcpConnectConfig == tcpConnectConfig)
                {
                    return;
                }

                _tcpConnectConfig = tcpConnectConfig;
            }

            _watchTimer.Run();
        }

        public void SetAcceptConfig(TcpAcceptConfig tcpAcceptConfig)
        {
            lock (_lockObject)
            {
                if (_tcpAcceptConfig == tcpAcceptConfig)
                {
                    return;
                }

                _tcpAcceptConfig = tcpAcceptConfig;
            }

            _watchTimer.Run();
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

                foreach (var tcpListener in _tcpListeners.Randomize())
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

        private void WatchListenerThread()
        {
            try
            {
                for (; ; )
                {
                    var config = _tcpAcceptConfig;

                    // 
                    if (config == null || !config.Enabled)
                    {

                    }
                    else
                    {
                        foreach (var omniAddress in config.ListenAddresses)
                        {
                            IPAddress ipAddress;
                            ushort port;

                            if (!TryGetEndpoint(omniAddress, out ipAddress, out port, false))
                            {
                                continue;
                            }


                        }
                    }

                    if (config..Type.HasFlag(TcpConnectionType.Ipv4) && config.Ipv4Port != 0)
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
                        if (this.Config != config)
                        {
                            continue;
                        }

                        _locationUris.Clear();
                        if (ipv4Uri != null)
                        {
                            _locationUris.Add(ipv4Uri);
                        }

                        if (ipv6Uri != null)
                        {
                            _locationUris.Add(ipv6Uri);
                        }
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

        public override ServiceStateType StateType { get; }

        private readonly object _stateLockObject = new object();

        public override void Start()
        {
            lock (_stateLockObject)
            {
                lock (_lockObject)
                {
                    if (this.State == ManagerState.Start)
                    {
                        return;
                    }

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
                    if (this.State == ManagerState.Stop)
                    {
                        return;
                    }

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
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (disposing)
            {

            }
        }

        public override ValueTask Restart()
        {
            throw new NotImplementedException();
        }
    }
}
