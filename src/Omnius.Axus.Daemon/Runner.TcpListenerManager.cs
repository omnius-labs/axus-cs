using System.Net.Sockets;
using Omnius.Core;
using Omnius.Core.Net;

namespace Omnius.Axus.Daemon;

public static partial class Runner
{
    private sealed class TcpListenerManager : DisposableBase
    {
        private readonly TcpListener _tcpListener;
        private readonly CancellationTokenRegistration _registration;

        public TcpListenerManager(OmniAddress listenAddress, CancellationToken cancellationToken = default)
        {
            if (!listenAddress.TryParseTcpEndpoint(out var ipAddress, out var port)) throw new Exception("listenAddress is invalid format.");

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
                _tcpListener.Stop();
                _tcpListener.Server.Dispose();
            }
        }
    }
}
