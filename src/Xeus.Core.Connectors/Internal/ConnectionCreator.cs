using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Base;
using Omnix.Network;
using Xeus.Core.Internal.Connection.Primitives;

namespace Xeus.Core.Internal.Connection
{
    internal sealed class ConnectionCreator : ServiceBase, ISettings, IConnector
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly BufferPool _bufferPool;
        private readonly TcpConnectionCreator _tcpConnectionCreator;

        private readonly object _lockObject = new object();

        public ConnectionCreator(string basePath, BufferPool bufferPool)
        {
            var settingsPath = Path.Combine(basePath, "Settings");
            var childrenPath = Path.Combine(basePath, "Children");

            _bufferPool = bufferPool;
            _tcpConnectionCreator = new TcpConnectionCreator(Path.Combine(childrenPath, nameof(TcpConnectionCreator)), bufferPool);
        }

        public void SetOptions(ConnectorOptions options)
        {
            lock (_lockObject)
            {
                _tcpConnectionCreator.SetTcpConnectOptions(options.TcpConnectOptions);
                _tcpConnectionCreator.SetTcpAcceptOptions(options.TcpAcceptOptions);
            }
        }

        public async ValueTask<ConnectorResult> ConnectAsync(OmniAddress address, CancellationToken token = default)
        {
            if (this.IsDisposed)
            {
                return new ConnectorResult(ConnectorResultType.Failed, null, null);
            }

            if (this.StateType != ServiceStateType.Running)
            {
                return new ConnectorResult(ConnectorResultType.Failed, null, null);
            }

            Cap? result;

            if ((result = await _tcpConnectionCreator.ConnectAsync(address, token)) != null)
            {
                return new ConnectorResult(ConnectorResultType.Succeeded, result, address);
            }

            return new ConnectorResult(ConnectorResultType.Failed, null, null);
        }

        public async ValueTask<ConnectorResult> AcceptAsync(CancellationToken token = default)
        {
            if (this.IsDisposed)
            {
                return new ConnectorResult(ConnectorResultType.Failed, null, null);
            }

            if (this.StateType != ServiceStateType.Running)
            {
                return new ConnectorResult(ConnectorResultType.Failed, null, null);
            }

            ConnectionCreatorAcceptResult? result;

            if ((result = await _tcpConnectionCreator.AcceptAsync(token)) != null)
            {
                return result;
            }

            return null;
        }

        protected override async ValueTask OnInitializeAsync()
        {
        }

        protected override async ValueTask OnStartAsync()
        {
            this.StateType = ServiceStateType.Starting;

            await _tcpConnectionCreator.StartAsync();

            this.StateType = ServiceStateType.Running;
        }

        protected override async ValueTask OnStopAsync()
        {
            this.StateType = ServiceStateType.Stopping;

            await _tcpConnectionCreator.StopAsync();

            this.StateType = ServiceStateType.Stopped;
        }

        public async ValueTask LoadAsync()
        {
            await _tcpConnectionCreator.LoadAsync();
        }

        public async ValueTask SaveAsync()
        {
            await _tcpConnectionCreator.SaveAsync();
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                _tcpConnectionCreator.Dispose();
            }
        }
    }
}
