using System.Net;
using System.Net.Sockets;
using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Core.Net.Upnp;

namespace Omnius.Xeus.Engines;

public sealed partial class TcpConnectionAccepter
{
    private sealed class TcpListenerManager : AsyncDisposableBase
    {
        private readonly bool _useUpnp;
        private readonly IUpnpClientFactory _upnpClientFactory;

        private TcpListener? _tcpListener;

        public static async ValueTask<TcpListenerManager> CreateAsync(OmniAddress listenAddress, bool useUpnp, IUpnpClientFactory upnpClientFactory, CancellationToken cancellationToken = default)
        {
            var tcpListenerManager = new TcpListenerManager(useUpnp, upnpClientFactory);
            await tcpListenerManager.InitAsync(listenAddress, cancellationToken);
            return tcpListenerManager;
        }

        private TcpListenerManager(bool useUpnp, IUpnpClientFactory upnpClientFactory)
        {
            _useUpnp = useUpnp;
            _upnpClientFactory = upnpClientFactory;
        }

        private async ValueTask InitAsync(OmniAddress listenAddress, CancellationToken cancellationToken = default)
        {
            IUpnpClient? upnpClient = null;

            try
            {
                // TcpListenerの追加処理
                if (!listenAddress.TryGetTcpEndpoint(out var ipAddress, out ushort port, false)) return;

                _tcpListener = new TcpListener(ipAddress, port);
                _tcpListener.Start(3);

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
                if (_tcpListener is not null)
                {
                    var ipEndpoint = (IPEndPoint)_tcpListener.LocalEndpoint;

                    _tcpListener.Stop();

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

            if (_tcpListener is null || !_tcpListener.Pending()) return null;
            var socket = _tcpListener.AcceptSocket();
            return socket;
        }
    }
}
