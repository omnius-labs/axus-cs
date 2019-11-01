using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Base;
using Omnix.Network;
using Xeus.Core.Connectors.Internal;

namespace Xeus.Core.Connectors
{
    internal sealed class XeusConnector : ServiceBase, IConnector
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IBufferPool<byte> _bufferPool;
        private readonly TcpConnector _tcpConnector;

        private readonly object _lockObject = new object();

        public XeusConnector(string basePath, IBufferPool<byte> bufferPool)
        {
            var settingsPath = Path.Combine(basePath, "Settings");
            var childrenPath = Path.Combine(basePath, "Children");

            _bufferPool = bufferPool;
            _tcpConnector = new TcpConnector(Path.Combine(childrenPath, nameof(TcpConnector)), bufferPool);
        }

        public void SetOptions(ConnectorOptions options)
        {
            lock (_lockObject)
            {
                _tcpConnector.SetTcpConnectOptions(options.TcpConnectOptions);
                _tcpConnector.SetTcpAcceptOptions(options.TcpAcceptOptions);
            }
        }

        public async ValueTask<ConnectorResult> ConnectAsync(OmniAddress address, CancellationToken token = default)
        {
            if (this.IsDisposed)
            {
                return new ConnectorResult(ConnectorResultType.Failed);
            }

            if (this.StateType != ServiceStateType.Running)
            {
                return new ConnectorResult(ConnectorResultType.Failed);
            }

            var result = await _tcpConnector.ConnectAsync(address, token);

            if (result.Type == ConnectorResultType.Succeeded)
            {
                return result;
            }

            return new ConnectorResult(ConnectorResultType.Failed);
        }

        public async ValueTask<ConnectorResult> AcceptAsync(CancellationToken token = default)
        {
            if (this.IsDisposed)
            {
                return new ConnectorResult(ConnectorResultType.Failed);
            }

            if (this.StateType != ServiceStateType.Running)
            {
                return new ConnectorResult(ConnectorResultType.Failed);
            }

            var result = await _tcpConnector.AcceptAsync(token);

            if (result.Type == ConnectorResultType.Succeeded)
            {
                return result;
            }

            return new ConnectorResult(ConnectorResultType.Failed);
        }

        protected override async ValueTask OnInitializeAsync()
        {
        }

        protected override async ValueTask OnStartAsync()
        {
            this.StateType = ServiceStateType.Starting;

            await _tcpConnector.StartAsync();

            this.StateType = ServiceStateType.Running;
        }

        protected override async ValueTask OnStopAsync()
        {
            this.StateType = ServiceStateType.Stopping;

            await _tcpConnector.StopAsync();

            this.StateType = ServiceStateType.Stopped;
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                _tcpConnector.Dispose();
            }
        }
    }
}
