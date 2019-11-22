using System;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Network;

namespace Omnius.Xeus.Engine
{
    public interface IConnectorAggregator : ITcpConnector
    {

    }

    public interface IPrimitiveConnector : IDisposable
    {
        ValueTask<ConnectorResult> AcceptAsync(CancellationToken token = default);
        ValueTask<ConnectorResult> ConnectAsync(OmniAddress address, CancellationToken token = default);
    }

    public interface ITcpConnector : IPrimitiveConnector
    {
        TcpAcceptOptions TcpAcceptOptions { get; }
        TcpConnectOptions TcpConnectOptions { get; }

        void SetTcpAcceptOptions(TcpAcceptOptions? tcpAcceptConfig);
        void SetTcpConnectOptions(TcpConnectOptions? tcpConnectConfig);
    }
}
