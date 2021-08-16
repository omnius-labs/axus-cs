using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Core.Net.Upnp;

namespace Omnius.Xeus.Engines
{
    public sealed partial class TcpConnectionAccepter
    {
        private sealed class TcpListenerManager : AsyncDisposableBase
        {
            private readonly bool _useUpnp;
            private readonly IUpnpClientFactory _upnpClientFactory;

            private readonly List<TcpListener> _tcpListeners = new();

            public TcpListenerManager(bool useUpnp, IUpnpClientFactory upnpClientFactory)
            {
                _useUpnp = useUpnp;
                _upnpClientFactory = upnpClientFactory;
            }

            public static async ValueTask<TcpListenerManager> CreateAsync(IEnumerable<OmniAddress> listenAddresses, bool useUpnp, IUpnpClientFactory upnpClientFactory, CancellationToken cancellationToken = default)
            {
                var tcpListenerManager = new TcpListenerManager(useUpnp, upnpClientFactory);
                await tcpListenerManager.InitAsync(listenAddresses, cancellationToken);
                return tcpListenerManager;
            }

            private async ValueTask InitAsync(IEnumerable<OmniAddress> listenAddresses, CancellationToken cancellationToken = default)
            {
                var listenAddressSet = new HashSet<OmniAddress>(listenAddresses);

                IUpnpClient? upnpClient = null;

                try
                {
                    // TcpListenerの追加処理
                    foreach (var listenAddress in listenAddressSet)
                    {
                        if (!listenAddress.TryGetTcpEndpoint(out var ipAddress, out ushort port, false)) continue;

                        var tcpListener = new TcpListener(ipAddress, port);
                        tcpListener.Start(3);

                        _tcpListeners.Add(tcpListener);

                        if (_useUpnp)
                        {
                            // "0.0.0.0"以外はUPnPでのポート開放対象外
                            if (ipAddress == IPAddress.Any)
                            {
                                if (upnpClient == null)
                                {
                                    upnpClient = _upnpClientFactory.Create();
                                    await upnpClient.ConnectAsync(cancellationToken);
                                }

                                await upnpClient.OpenPortAsync(UpnpProtocolType.Tcp, port, port, "Xeus", cancellationToken);
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

            protected override async ValueTask OnDisposeAsync()
            {
                IUpnpClient? upnpClient = null;

                try
                {
                    foreach (var tcpListener in _tcpListeners)
                    {
                        var ipEndpoint = (IPEndPoint)tcpListener.LocalEndpoint;

                        tcpListener.Stop();

                        if (_useUpnp)
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

            public async ValueTask<Socket?> AcceptAsync(CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var tcpListener in _tcpListeners)
                {
                    if (!tcpListener.Pending()) continue;
                    var socket = tcpListener.AcceptSocket();
                    return socket;
                }

                return null;
            }
        }
    }
}
