using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Network;
using Omnius.Xeus.Engine.Connectors.Internal;

namespace Omnius.Xeus.Engine.Connectors
{
    internal sealed class XeusConnector : ServiceBase, IConnectorAggregator
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IBufferPool<byte> _bufferPool;
        private readonly TcpConnector _tcpConnector;

        private readonly object _lockObject = new object();

        public XeusConnector(string basePath, IBufferPool<byte> bufferPool)
        {
            var configPath = Path.Combine(basePath, "config");
            var refsPath = Path.Combine(basePath, "refs");

            _bufferPool = bufferPool;
            _tcpConnector = new TcpConnector(Path.Combine(refsPath, nameof(TcpConnector)), bufferPool);
        }

        public TcpAcceptOptions TcpAcceptOptions { get; }
        public TcpConnectOptions TcpConnectOptions { get; }

        public void SetTcpAcceptOptions(TcpAcceptOptions? tcpAcceptConfig)
        {
            _tcpConnector.SetTcpAcceptOptions(tcpAcceptConfig);
        }

        public void SetTcpConnectOptions(TcpConnectOptions? tcpConnectConfig)
        {
            _tcpConnector.SetTcpConnectOptions(tcpConnectConfig);
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
