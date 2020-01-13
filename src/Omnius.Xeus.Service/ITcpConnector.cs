using System;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public interface ITcpConnectorFactory
    {
        public ValueTask<ITcpConnector> Create(TcpConnectorOptions tcpConnectorOptions, IBufferPool<byte> bufferPool);
    }

    public interface ITcpConnector : IConnector, IAsyncDisposable
    {
        public static ITcpConnectorFactory Factory { get; }
    }
}
