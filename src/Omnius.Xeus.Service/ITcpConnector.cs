using System;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public interface ITcpConnectorFactory
    {
        ValueTask<ITcpConnector> Create(TcpConnectorOptions tcpConnectorOptions, IBufferPool<byte> bufferPool);
    }

    public interface ITcpConnector : IPrimitiveConnector, IAsyncDisposable
    {
        public static ITcpConnectorFactory Factory { get; }
    }
}
