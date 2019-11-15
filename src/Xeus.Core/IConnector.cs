using System.Threading;
using System.Threading.Tasks;
using Omnix.Base;
using Omnix.Network;

namespace Xeus.Core
{
    public interface IConnectorAggregator : ITcpConnector
    {

    }

    public interface IPrimitiveConnector
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
