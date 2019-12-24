using Omnius.Core;
using Omnius.Xeus.Service.Components.Primitives;

namespace Omnius.Xeus.Service.Components
{
    public interface ITcpConnector : IPrimitiveConnector
    {
        TcpAcceptOptions TcpAcceptOptions { get; }
        TcpConnectOptions TcpConnectOptions { get; }

        void SetTcpAcceptOptions(TcpAcceptOptions tcpAcceptConfig);
        void SetTcpConnectOptions(TcpConnectOptions tcpConnectConfig);
    }
}
