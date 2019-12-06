using Omnius.Core;

namespace Omnius.Xeus.Engine
{
    public interface ITcpConnector : IPrimitiveConnector
    {
        TcpAcceptOptions TcpAcceptOptions { get; }
        TcpConnectOptions TcpConnectOptions { get; }

        void SetTcpAcceptOptions(TcpAcceptOptions? tcpAcceptConfig);
        void SetTcpConnectOptions(TcpConnectOptions? tcpConnectConfig);
    }
}
