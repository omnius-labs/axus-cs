using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Core.Net.Caps;
using Omnius.Core.Net.Connections.Bridge;
using Omnius.Core.Net.Connections.Multiplexer;
using Omnius.Core.RocketPack.Remoting;
using Omnius.Core.Tasks;
using Omnius.Xeus.Service.Remoting;
using MultiplexerV1 = Omnius.Core.Net.Connections.Multiplexer.V1;

namespace Omnius.Xeus.Ui.Desktop.Internal
{
    internal class XeusServiceManager : AsyncDisposableBase
    {
        private Socket? _socket;
        private SocketCap? _cap;
        private BatchActionDispatcher? _batchActionDispatcher;
        private BridgeConnection? _bridgeConnection;
        private OmniConnectionMultiplexer? _multiplexer;
        private XeusServiceRemoting.Client<DefaultErrorMessage>? _xeusServiceRemotingClient;

        public XeusServiceManager()
        {
        }

        public async ValueTask ConnectAsync(OmniAddress address, IBytesPool bytesPool, CancellationToken cancellationToken = default)
        {
            if (!address.TryGetTcpEndpoint(out var ipAddress, out var port))
            {
                throw new Exception("address is invalid format.");
            }

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await _socket.ConnectAsync(new IPEndPoint(ipAddress, port), TimeSpan.FromSeconds(3), cancellationToken);

            _cap = new SocketCap(_socket);

            _batchActionDispatcher = new BatchActionDispatcher(TimeSpan.FromMilliseconds(10));
            var bridgeConnectionOptions = new BridgeConnectionOptions(int.MaxValue);
            _bridgeConnection = new BridgeConnection(_cap, null, null, _batchActionDispatcher, bytesPool, bridgeConnectionOptions);

            var multiplexerOptions = new MultiplexerV1.OmniConnectionMultiplexerOptions(OmniConnectionMultiplexerType.Connected, TimeSpan.FromMilliseconds(1000 * 10), 10, uint.MaxValue, 10);
            _multiplexer = OmniConnectionMultiplexer.CreateV1(_bridgeConnection, _batchActionDispatcher, bytesPool, multiplexerOptions);

            await _multiplexer.HandshakeAsync(cancellationToken);

            var rocketRemotingCallerFactory = new RocketRemotingCallerFactory<DefaultErrorMessage>(_multiplexer, bytesPool);
            _xeusServiceRemotingClient = new XeusServiceRemoting.Client<DefaultErrorMessage>(rocketRemotingCallerFactory, bytesPool);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            if (_multiplexer is not null) await _multiplexer.DisposeAsync();
            if (_bridgeConnection is not null) await _bridgeConnection.DisposeAsync();
            if (_batchActionDispatcher is not null) await _batchActionDispatcher.DisposeAsync();
            _cap?.Dispose();
            _socket?.Dispose();
        }

        public IXeusService GetService() => _xeusServiceRemotingClient ?? throw new NullReferenceException();
    }
}
