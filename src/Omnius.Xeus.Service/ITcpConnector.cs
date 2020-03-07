using System;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public interface ITcpConnectorFactory
    {
        public ValueTask<ITcpConnector> CreateAsync(TcpConnectorOptions tcpConnectorOptions, IBytesPool bytesPool);
    }

    public interface ITcpConnector : IConnector, IAsyncDisposable
    {
    }
}
