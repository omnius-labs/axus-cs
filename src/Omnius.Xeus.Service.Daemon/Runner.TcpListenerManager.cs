using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Net;

namespace Omnius.Xeus.Service.Daemon
{
    public static partial class Runner
    {
        private sealed class TcpListenerManager : DisposableBase
        {
            private readonly TcpListener _tcpListener;
            private readonly CancellationTokenRegistration _registration;

            public TcpListenerManager(string listenAddress, CancellationToken cancellationToken = default)
            {
                var listenOmniAddress = new OmniAddress(listenAddress);
                if (!listenOmniAddress.TryGetTcpEndpoint(out var ipAddress, out var port)) throw new Exception("listenAddress is invalid format.");

                _tcpListener = new TcpListener(ipAddress!, port);
                _tcpListener.Start();
                _registration = cancellationToken.Register(() => _tcpListener.Stop());
            }

            public async ValueTask<Socket> AcceptSocketAsync()
            {
                return await _tcpListener.AcceptSocketAsync();
            }

            protected override void OnDispose(bool disposing)
            {
                _registration.Dispose();

                if (_tcpListener is not null)
                {
                    _tcpListener.Server.Dispose();
                }
            }
        }
    }
}
